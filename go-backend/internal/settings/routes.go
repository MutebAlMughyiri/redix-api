// Package settings keeps a project's payment methods (UC-014, FR-40): added, renamed, disabled and enabled, and deleted while no expense was paid
// with them (PaymentMethodsEndpoints, redix-api #140).
package settings

import (
	"net/http"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db/store"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/problem"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/google/uuid"
)

// Routes keeps a project's payment methods.
type Routes struct {
	database db.DB
	writer   changes.Writer
}

// Map routes the payment methods: reading them for whoever reads the project, changing them for whoever holds ManageSettings in a project not
// finished (AC-04.1).
func Map(router *web.Router, gate *access.Gate, database db.DB, clock clock.Clock) {
	s := Routes{database: database, writer: changes.Writer{Database: database, Clock: clock}}
	const methods = "/projects/{projectId:guid}/payment-methods"
	changes := func(handler access.Handler) http.HandlerFunc { return gate.Changes(access.ManageSettings, handler) }
	router.Handle("GET", methods, gate.Needs(access.ReadProject, s.list(false)))
	router.Handle("GET", methods+"/offered", gate.Needs(access.ReadProject, s.list(true)))
	router.Handle("POST", methods, changes(s.add))
	router.Handle("PUT", methods+"/{paymentMethodId:guid}", changes(s.rename))
	router.Handle("POST", methods+"/{paymentMethodId:guid}/disable", changes(s.activate(false)))
	router.Handle("POST", methods+"/{paymentMethodId:guid}/enable", changes(s.activate(true)))
	router.Handle("DELETE", methods+"/{paymentMethodId:guid}", changes(s.delete))
}

// paymentMethodView is a payment method as the app shows it (PaymentMethodView).
type paymentMethodView struct {
	ID       uuid.UUID `json:"id"`
	Name     string    `json:"name"`
	IsActive bool      `json:"isActive"`
}

// list is the project's payment methods in order: every one, or only the active ones an entry may choose from (AC-40.1).
func (s Routes) list(activeOnly bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, _ access.Caller) {
		methods, err := s.database.Queries().PaymentMethodsOf(r.Context(), web.ID(r, "projectId"))
		if err != nil {
			problem.Status(w, r, http.StatusInternalServerError)
			return
		}
		views := []paymentMethodView{}
		for _, method := range methods {
			if method.IsActive || !activeOnly {
				views = append(views, paymentMethodView{ID: method.ID, Name: method.Name, IsActive: method.IsActive})
			}
		}
		web.JSON(w, r, http.StatusOK, views)
	}
}

func fields(method store.PaymentMethod) map[string]any {
	return map[string]any{"ProjectId": method.ProjectID, "Name": method.Name, "SortOrder": method.SortOrder, "IsActive": method.IsActive}
}

func save(c changes.Change, before, after store.PaymentMethod) error {
	return c.Save("PaymentMethods", after.ID, before.ID == uuid.Nil, fields(before), fields(after),
		func() error { return c.Q.SavePaymentMethod(c.Ctx, store.SavePaymentMethodParams(after)) })
}

// add adds a payment method at the end of the project's list.
func (s Routes) add(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	name, ok := changes.Named(w, r)
	if !ok {
		return
	}
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		methods, err := c.Q.PaymentMethodsOf(c.Ctx, c.ProjectID)
		if err != nil {
			return err
		}
		place := int64(0)
		for _, method := range methods {
			place = max(place, method.SortOrder+1)
		}
		return save(c, store.PaymentMethod{}, store.PaymentMethod{ID: uuid.Must(uuid.NewV7()), ProjectID: c.ProjectID, Name: name, SortOrder: place, IsActive: true})
	})
}

// change applies apply to the route's payment method: 404 for one the project does not hold.
func (s Routes) change(w http.ResponseWriter, r *http.Request, caller access.Caller, apply func(*store.PaymentMethod)) {
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		before, err := changes.Found(c.Q.PaymentMethodOf(c.Ctx, store.PaymentMethodOfParams{ID: web.ID(r, "paymentMethodId"), ProjectID: c.ProjectID}))
		if err != nil {
			return err
		}
		after := before
		apply(&after)
		return save(c, before, after)
	})
}

func (s Routes) rename(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	if name, ok := changes.Named(w, r); ok {
		s.change(w, r, caller, func(method *store.PaymentMethod) { method.Name = name })
	}
}

// activate disables a payment method, so it leaves entry and stays on the entries that used it (AC-40.1), or brings it back.
func (s Routes) activate(active bool) access.Handler {
	return func(w http.ResponseWriter, r *http.Request, caller access.Caller) {
		s.change(w, r, caller, func(method *store.PaymentMethod) { method.IsActive = active })
	}
}

// delete deletes a payment method no expense was paid with: paymentMethodUsed (409) for one an expense keeps, which is disabled instead.
func (s Routes) delete(w http.ResponseWriter, r *http.Request, caller access.Caller) {
	s.writer.Done(w, r, caller, func(c changes.Change) error {
		method, err := changes.Found(c.Q.PaymentMethodOf(c.Ctx, store.PaymentMethodOfParams{ID: web.ID(r, "paymentMethodId"), ProjectID: c.ProjectID}))
		if err != nil {
			return err
		}
		if used, err := c.Q.PaymentMethodUsed(c.Ctx, method.ID); err != nil || used {
			if used {
				return changes.Refuse(http.StatusConflict, "paymentMethodUsed")
			}
			return err
		}
		return c.Remove("PaymentMethods", method.ID, fields(method), func() error { return c.Q.RemovePaymentMethod(c.Ctx, method.ID) })
	})
}

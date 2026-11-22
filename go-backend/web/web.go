// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package web is REDIX's web front end, the second door into the same rules (redix-api #80, #161): pages this binary draws inside one shell, the
// base design the owner approved on 21 Nov 2026, each calling the owner the app's route calls. Every web file lives in this folder; the backend's
// packages hold none (#161 S14).
package web

import (
	"net/http"
	"runtime/debug"

	"github.com/a-h/templ"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/attachments"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/auth"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/db"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/legal"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/clock"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/limits"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/statement"
)

// Version is the web's version, SemVer, raised by hand with each release as pubspec raises the app's (#161 S12).
const Version = "1.0.0"

// build is the web's build beside its version, " (b6ef37d)": the commit this program was built from, which Go stamps into every build made in
// the repository, so a tester tells one deploy from the next with no step to forget (the owner, 21 Nov 2026: "add web build number to the login
// page"). "" in a build with no commit stamped, as a test's.
var build = func() string {
	info, _ := debug.ReadBuildInfo()
	for _, setting := range info.Settings {
		if setting.Key == "vcs.revision" {
			return " (" + setting.Value[:min(7, len(setting.Value))] + ")"
		}
	}
	return ""
}()

// Parts are what the pages run on: the same database, sessions, codes, gate and access book as the app's API, and the same windows that cap how
// often one address asks for an emailed code and registers a project.
type Parts struct {
	Database      db.DB
	Sessions      auth.Sessions
	Codes         auth.Codes
	EmailCodes    *limits.Window
	Registrations *limits.Window
	Gate          *access.Gate
	Book          *access.Book
	Clock         clock.Clock
	Typst         statement.Typst
	Links         attachments.Links
	ByLink        http.HandlerFunc
}

// site draws the pages.
type site struct {
	Parts
	assets assets
}

// Routes is every page of the web, behind Go's own cross-origin check, which refuses a form another site posts (#161 R17).
func Routes(parts Parts) http.Handler {
	s := site{Parts: parts, assets: loadAssets()}
	mux := http.NewServeMux()
	mux.HandleFunc("GET /{$}", s.home)
	mux.HandleFunc("GET /assets/{file}", s.assets.serve)
	mux.HandleFunc("GET /sign-in", s.signInAgain)
	mux.HandleFunc("POST /sign-in", s.signIn)
	mux.HandleFunc("GET /forgot-password", s.forgotPassword)
	mux.HandleFunc("POST /forgot-password", s.sendResetCode)
	mux.HandleFunc("POST /reset-with-code", s.resetWithCode)
	mux.HandleFunc("GET /register", s.registerForm)
	mux.HandleFunc("POST /register", s.register)
	mux.HandleFunc("POST /register/{list}", s.listed)
	mux.HandleFunc("POST /change-password", s.changePassword)
	mux.HandleFunc("POST /sign-out", s.signOut)
	mux.HandleFunc("GET /tab/{name}", s.openTab)
	mux.HandleFunc("DELETE /tab/{name}", s.closeTab)
	mux.HandleFunc("POST /view", s.keepView)
	mux.HandleFunc("GET /details/entry/{id}", s.entryDetails)
	mux.HandleFunc("POST /entry/{id}/{action}", s.entryAction)
	mux.HandleFunc("POST /entry", s.recordEntry)
	mux.HandleFunc("POST /entry/{id}", s.changeEntry)
	mux.HandleFunc("GET /entry-figures", s.entryFigures)
	mux.HandleFunc("POST /approvals/approve", s.approveSelected)
	mux.HandleFunc("GET /approval-history", s.approvalHistoryMore)
	mux.HandleFunc("GET /details/account/{id}", s.accountDetails)
	mux.HandleFunc("POST /account/{id}/{action}", s.accountAction)
	mux.HandleFunc("GET /details/account/new", s.newAccountForm)
	mux.HandleFunc("POST /account", s.keepAccount)
	mux.HandleFunc("POST /account/{id}/edit", s.keepAccount)
	mux.HandleFunc("GET /details/transfer", s.transferDetails)
	mux.HandleFunc("POST /transfer", s.transfer)
	mux.HandleFunc("GET /count-figures", s.countFigures)
	mux.HandleFunc("POST /count", s.keepCount)
	mux.HandleFunc("GET /account-history", s.accountHistoryMore)
	mux.HandleFunc("GET /details/partner/{id}", s.partnerDetails)
	mux.HandleFunc("GET /details/supplier/{id}", s.supplierDetailsPanel)
	mux.HandleFunc("GET /details/supplier/new", s.newSupplierForm)
	mux.HandleFunc("POST /supplier", s.keepSupplier)
	mux.HandleFunc("POST /supplier/{id}/{action}", s.keepSupplier)
	mux.HandleFunc("GET /details/contract/{id}", s.contractDetails)
	mux.HandleFunc("GET /details/contract/new", s.newContractForm)
	mux.HandleFunc("POST /contract", s.keepContract)
	mux.HandleFunc("POST /contract/{id}/{action}", s.keepContract)
	mux.HandleFunc("POST /reports/issue", s.issueReport)
	mux.HandleFunc("GET /report-file/periodic/{number}/{format}", s.periodicFile)
	mux.HandleFunc("GET /report-file/{kind}/{id}/{format}", s.reportFile)
	mux.HandleFunc("GET /files/{link}", s.ByLink)
	mux.HandleFunc("GET /project-history", s.projectHistoryMore)
	mux.HandleFunc("GET /my-history", s.myHistoryMore)
	mux.HandleFunc("GET /details/user/{id}", s.userDetails)
	mux.HandleFunc("GET /details/user/existing", s.existingUserForm)
	mux.HandleFunc("POST /user", s.keepUser)
	mux.HandleFunc("POST /users/new", s.createUser)
	mux.HandleFunc("POST /user/{id}/permission/{permission}", s.keepPermission)
	mux.HandleFunc("POST /user/{id}/partner/{partner}", s.keepChosenPartner)
	mux.HandleFunc("POST /user/{id}/tables", s.keepTables)
	mux.HandleFunc("POST /permission-request/withdraw", s.withdrawRequest)
	mux.HandleFunc("GET /details/structure/{kind}/{id}", s.structureDetails)
	mux.HandleFunc("POST /structure/{kind}/{id}/{action}", s.keepStructure)
	mux.HandleFunc("POST /structure/reorder", s.reorderStructure)
	mux.HandleFunc("POST /structure/category/{id}/reorder", s.reorderStructure)
	mux.HandleFunc("GET /details/columns/{item}/{scope}/{id}", s.columnDetails)
	mux.HandleFunc("POST /columns/{item}/{scope}/{id}/{action}", s.keepColumn)
	mux.HandleFunc("POST /columns/{item}/{scope}/reorder", s.reorderColumns)
	mux.HandleFunc("GET /details/tables/{item}/{id}", s.tableDetails)
	mux.HandleFunc("POST /tables/{item}/{id}/{action}", s.keepTable)
	mux.HandleFunc("POST /tables/{item}/reorder", s.reorderTables)
	mux.HandleFunc("GET /details/payment-method/{id}", s.methodDetails)
	mux.HandleFunc("GET /details/project-info/{asking}", s.projectInfoDetails)
	mux.HandleFunc("GET /notifications/count", s.notificationsButton)
	mux.HandleFunc("GET /theme/{theme}", s.chooseTheme)
	mux.HandleFunc("POST /entries-shown", s.chooseEntriesShown)
	mux.HandleFunc("GET /privacy", legal.Privacy())
	mux.HandleFunc("GET /terms", legal.Terms())
	mux.HandleFunc("GET /notification/{id}/open", s.openNotification)
	mux.HandleFunc("POST /project-info/{asking}", s.keepProjectInfo)
	mux.HandleFunc("POST /payment-method/{id}/{action}", s.keepMethod)
	mux.HandleFunc("GET /details/permission-request", s.askDialog)
	mux.HandleFunc("POST /permission-request", s.sendRequest)
	mux.HandleFunc("GET /details/permission-request/{id}/reject", s.rejectDialog)
	mux.HandleFunc("POST /permission-request/{id}/{action}", s.decideRequest)
	mux.HandleFunc("POST /user/{id}/{action}", s.keepUser)
	mux.HandleFunc("GET /partner-history", s.partnerHistoryMore)
	mux.HandleFunc("GET /details/partner/new", s.newPartnerForm)
	mux.HandleFunc("POST /partner", s.keepPartner)
	mux.HandleFunc("POST /partner/{id}/{action}", s.keepPartner)
	mux.HandleFunc("POST /project/{id}", s.chooseProject)
	mux.HandleFunc("POST /project/{id}/default", s.starProject)
	mux.HandleFunc("GET /language/{language}", s.chooseLanguage)
	return http.NewCrossOriginProtection().Handler(mux)
}

// draw answers status with components, one after another: a page holds a person's data, so no answer is kept by a cache.
func draw(w http.ResponseWriter, r *http.Request, status int, components ...templ.Component) {
	w.Header().Set("Content-Type", "text/html; charset=utf-8")
	w.Header().Set("Cache-Control", "no-store")
	w.WriteHeader(status)
	for _, component := range components {
		_ = component.Render(r.Context(), w)
	}
}

// failed answers a database that failed, as the API answers it: 500.
func failed(w http.ResponseWriter) {
	http.Error(w, http.StatusText(http.StatusInternalServerError), http.StatusInternalServerError)
}

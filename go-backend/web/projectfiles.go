// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package web

import (
	"errors"
	"net/http"
	"strconv"

	"github.com/a-h/templ"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/access"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/archive"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/attachments"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/changes"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/web"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/projectfile"
	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/zones"
)

// mayLoadFile says whether p's caller may load a project file: ManageProjectFiles, in a project not finished that holds no entry yet, as the
// app's Project.mayLoadFile (AC-53.3).
func (p page) mayLoadFile() bool { return p.mayRetrieve() && !p.project.HoldsEntries }

// mayRetrieve says whether p's caller may retrieve an archive: ManageProjectFiles, in a project not finished, as the app's Project.mayRetrieve
// (UC-073 R17).
func (p page) mayRetrieve() bool { return !p.finished() && p.may(access.ManageProjectFiles) }

// loadFileGray is why Load file stands gray, in the words the API refuses it with; "" while it may load.
func (p page) loadFileGray() string {
	switch {
	case p.finished():
		return "projects.loadFile.projectFinished"
	case p.project.HoldsEntries:
		return "projects.loadFile.projectHasEntries"
	}
	return ""
}

// ownerWords are where each owner's words start, so a refused row says why in the words of the rule that refused it, as the app's
// ProjectFileOwner does (D14).
var ownerWords = map[projectfile.Owner]string{projectfile.OwnerFile: "projects.loadFile", projectfile.OwnerEntry: "expenses",
	projectfile.OwnerStructure: "categories", projectfile.OwnerContract: "contracts", projectfile.OwnerPartner: "partners",
	projectfile.OwnerSupplier: "suppliers", projectfile.OwnerPaymentMethod: "paymentMethods", projectfile.OwnerReconciliation: "reconciliations"}

// placeOf is where a refused row is, as the file and its workbook name it: the item, the table and «#» with the row's place.
func placeOf(refused projectfile.Refusal) string {
	row := ""
	if refused.Row > 0 {
		row = "#" + strconv.Itoa(refused.Row)
	}
	return parts(refused.Item, refused.Table, row)
}

// reasonOf is why a row was refused, in p's words: its owner's word for its code, with the value and the column it names in its places.
func reasonOf(p page, refused projectfile.Refusal) string {
	return p.words.Of(ownerWords[refused.Owner]+"."+refused.Code, refused.Value, refused.Column)
}

// loaded is what Load file says once it answered: what it loaded, why it refused, and every refused row.
type loaded struct {
	said
	rows []projectfile.Refusal
}

// loadFileTab is Load file in its tab (UC-071, redix-api #175 D3), for whoever handles the project's files.
func (s site) loadFileTab(_ *http.Request, p page) (templ.Component, error) {
	return loadFileView(p, loaded{}), nil
}

// loadFile loads the project file the form sent (POST /load-file) through the rule the app's route runs, and answers Load file again saying
// what it loaded, or why not in the app's words, with every refused row listed as the app's Refused screen lists them. 403 without
// ManageProjectFiles; 404 with no project open.
func (s site) loadFile(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageProjectFiles) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	content, _, err := attachments.FormFile(r)
	answer := loaded{said: said{refusal: p.loadFileGray()}}
	switch {
	case err != nil:
		answer.refusal = "documents.fileRequired"
	case answer.refusal == "":
		var done projectfile.Loaded
		err = changes.Writer{Database: s.Database, Clock: s.Clock}.Run(r.Context(), p.project.ID, p.caller, func(c changes.Change) (err error) {
			done, err = projectfile.Load(c, content)
			return err
		})
		refused, _ := errors.AsType[changes.Refusal](err)
		if answer.refusal, err = refusalOf("projects.loadFile", err); err != nil {
			failed(w)
			return
		}
		answer.rows, _ = refused.Detail.([]projectfile.Refusal)
		answer.notice = p.loadedWords(done, answer.refusal)
	}
	draw(w, r, http.StatusOK, loadFileView(p, answer))
}

// loadedWords is what Load file says it loaded, as the app's bar says it: the entries and tables, and the report when the file issued one; ""
// once refused.
func (p page) loadedWords(done projectfile.Loaded, refusal string) string {
	switch {
	case refusal != "":
		return ""
	case done.Report > 0:
		return p.words.Of("projects.loadFileDone", strconv.Itoa(done.Entries), strconv.Itoa(done.Tables), strconv.FormatInt(done.Report, 10))
	}
	return p.words.Of("projects.loadFileDoneNoReport", strconv.Itoa(done.Entries), strconv.Itoa(done.Tables))
}

// archiveFile hands over the project's archive (GET /project-file/archive, AC-73.1) through the rule the app's route runs, named as the app names
// it, «{project} - أرشيف {date time}» by the project's clock. 404 without ManageProjectFiles, as the side menu leaves Archive out.
func (s site) archiveFile(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	if !p.inProject() || !p.may(access.ManageProjectFiles) {
		http.NotFound(w, r)
		return
	}
	read := s.Database.Queries()
	zone, err := zones.Of(r.Context(), read, p.project.ID)
	if err != nil {
		failed(w)
		return
	}
	content, projectName, err := archive.Take(r.Context(), changes.Writer{Database: s.Database, Clock: s.Clock}, p.project.ID, p.caller)
	if err != nil {
		failed(w)
		return
	}
	at := s.Clock.Now().In(zones.Location(zone)).Format("2006-01-02 15.04")
	web.SendFile(w, "application/json", p.words.Of("projects.archiveFileName", projectName, at)+".json", content)
}

// retrieved is what Retrieve says: the preview of the archive chosen, what it retrieved, or why not, with the project an archive of another is
// of.
type retrieved struct {
	said
	preview archive.Retrieval
	when    string
	over    string
}

// retrieveTab is Retrieve in its tab (UC-073, redix-api #175 D3), for whoever handles the project's files.
func (s site) retrieveTab(_ *http.Request, p page) (templ.Component, error) {
	return retrieveView(p, retrieved{}), nil
}

// retrieveArchive answers the archive the form sent through the rule the app's route runs: its preview (POST /retrieve/preview) - when it was
// taken, by whom, and how many of each part the project holds now and after - asking to retrieve it, as the app's question asks; or the
// retrieve itself (POST /retrieve/archive), answering Retrieve again saying what it retrieved. Refused, it says why in the app's words, naming another
// project's archive. 403 without ManageProjectFiles; 404 with no project open; a finished project retrieves nothing (R17).
func (s site) retrieveArchive(w http.ResponseWriter, r *http.Request) {
	p, ok := s.working(w, r)
	if !ok {
		return
	}
	step := r.PathValue("step")
	if !p.inProject() || (step != "preview" && step != "archive") {
		http.NotFound(w, r)
		return
	}
	if !p.may(access.ManageProjectFiles) {
		http.Error(w, http.StatusText(http.StatusForbidden), http.StatusForbidden)
		return
	}
	preview := step == "preview"
	content, _, err := attachments.FormFile(r)
	answer := retrieved{said: said{refusal: "projects.retrieve.projectFinished"}}
	switch {
	case err != nil:
		answer.refusal = "documents.fileRequired"
	case !p.finished():
		answer.preview, err = archive.Retrieve(r.Context(), changes.Writer{Database: s.Database, Clock: s.Clock}, p.project.ID, p.caller, content, preview)
		refused, _ := errors.AsType[changes.Refusal](err)
		if answer.refusal, err = refusalOf("projects.retrieve", err); err != nil {
			failed(w)
			return
		}
		answer.over, _ = refused.Detail.(string)
	}
	zone, err := zones.Of(r.Context(), s.Database.Queries(), p.project.ID)
	if err != nil {
		failed(w)
		return
	}
	answer.when = answer.preview.ArchivedAt.In(zones.Location(zone)).Format("2006-01-02 15:04")
	if !preview && answer.refusal == "" {
		answer.notice = p.words.Of("projects.retrieveDone", answer.when, strconv.FormatInt(answer.preview.After["entries"], 10))
		draw(w, r, http.StatusOK, retrieveView(p, answer))
		return
	}
	draw(w, r, http.StatusOK, retrieveAnswer(p, answer))
}

// partLine is a part's line in the preview, as the app's question writes it: its name, how many the project holds now and after.
func partLine(p page, preview archive.Retrieval, part string) string {
	return p.words.Of("projects.retrieve.part."+part) + ": " + p.words.Of("projects.retrieveNow") + " " + strconv.FormatInt(preview.Now[part], 10) + " · " +
		p.words.Of("projects.retrieveAfter") + " " + strconv.FormatInt(preview.After[part], 10)
}

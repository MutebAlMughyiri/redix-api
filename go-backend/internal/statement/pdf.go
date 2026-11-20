// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package statement

import (
	"bytes"
	"embed"
	"encoding/json"
	"fmt"
	"math"
	"os"
	"os/exec"
	"path/filepath"
	"strconv"
	"time"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/platform/fixed"
)

// layout is the Typst template every report PDF is laid out by, and fonts the Arabic font it embeds (ReportFormats.FontFamily, NFR-13).
var (
	//go:embed report.typ
	layout []byte
	//go:embed fonts
	fonts embed.FS
)

// Document is a report PDF as the template lays it out: its pages, each a title and its blocks, every word and figure already written.
type Document struct {
	Pages []Page `json:"pages"`
}

// Page is one page: its title, in titleSize points and bold, then its blocks one under another.
type Page struct {
	Title     string  `json:"title"`
	TitleSize int     `json:"titleSize"`
	Blocks    []Block `json:"blocks"`
}

// Block is a heading, a line of a label and its figure, or a table, gap points under what is above it.
type Block struct {
	Kind   string       `json:"kind"`
	Gap    float64      `json:"gap"`
	Text   string       `json:"text"`
	Figure string       `json:"figure"`
	Strong bool         `json:"strong"`
	Widths []TableWidth `json:"widths,omitempty"`
	Header []string     `json:"header,omitempty"`
	Rows   []PdfRow     `json:"rows"`
}

// TableWidth is a column's width: so many points, or a share of what is left.
type TableWidth struct {
	Points float64 `json:"pt,omitempty"`
	Shares float64 `json:"fr,omitempty"`
}

// PdfRow is a row of a table, shaded green when it is new (BR-26).
type PdfRow struct {
	Cells []PdfCell `json:"cells"`
	IsNew bool      `json:"isNew"`
}

// PdfCell is a cell's text, bold when strong.
type PdfCell struct {
	Text   string `json:"text"`
	Strong bool   `json:"strong"`
}

// Heading is a bold heading, gap points under what is above it.
func Heading(text string, gap float64) Block { return Block{Kind: "heading", Text: text, Gap: gap} }

// Line is a label and its figure already written, bold when strong (PeriodicReportFiles.Line).
func Line(label, figure string, strong bool) Block {
	return Block{Kind: "line", Text: label, Figure: figure, Strong: strong}
}

// AmountLine is a label and an amount.
func AmountLine(label string, amount fixed.Number, strong bool) Block {
	return Line(label, Money(amount), strong)
}

// Typst makes PDFs with the typst binary at its path (#140 D2).
type Typst struct {
	Path string
}

// Pdf is the document as a PDF.
func (t Typst) Pdf(document Document) ([]byte, error) {
	folder, err := os.MkdirTemp("", "redix-pdf-")
	if err != nil {
		return nil, err
	}
	defer func() { _ = os.RemoveAll(folder) }()
	data, err := json.Marshal(document)
	if err == nil {
		err = os.WriteFile(filepath.Join(folder, "data.json"), data, 0o600)
	}
	if err == nil {
		err = os.WriteFile(filepath.Join(folder, "report.typ"), layout, 0o600)
	}
	for _, name := range []string{"IBMPlexSansArabic-Regular.ttf", "IBMPlexSansArabic-Bold.ttf"} {
		font, readErr := fonts.ReadFile("fonts/" + name)
		if err == nil {
			err = readErr
		}
		if err == nil {
			err = os.WriteFile(filepath.Join(folder, name), font, 0o600)
		}
	}
	if err != nil {
		return nil, err
	}
	var failure bytes.Buffer
	compile := exec.Command(t.Path, "compile", "--font-path", folder, "--ignore-system-fonts", "--root", folder,
		filepath.Join(folder, "report.typ"), filepath.Join(folder, "report.pdf"))
	compile.Stderr = &failure
	if err := compile.Run(); err != nil {
		return nil, fmt.Errorf("typst: %w: %s", err, failure.String())
	}
	return os.ReadFile(filepath.Join(folder, "report.pdf"))
}

// pdfNumberWidth is how wide a table's first column is on a PDF, in points: room for «رقم القيد» and an entry's number of five digits (AC-54.6).
const pdfNumberWidth = 40

// PdfPage is sheet on a PDF page, laid out as the workbook lays it (StatementPdf.Draw, findings row 130): its name, then each table with its title,
// rows and الاجمالي, المتبقي and مبلغ الاتفاق when an amount was agreed, and its percentage when the sheet calculates one; with more than one
// table, اجمالي الكل and نسبة الكل. A PDF has no formulas, so every figure the workbook leaves to one is worked out here.
func PdfPage(sheet Sheet) Page {
	page := Page{Title: sheet.Name, TitleSize: 13}
	var totals []fixed.Number
	whole := fixed.Number(0)
	for _, table := range sheet.Tables {
		total := fixed.Number(0)
		for _, row := range table.Rows {
			total += numberOf(row.Amount)
		}
		totals = append(totals, total)
		whole += total
	}
	if !sheet.ShowsEveryTable {
		whole = numberOf(sheet.Whole)
	}
	hasWhole := len(sheet.Tables) > 1 || !sheet.ShowsEveryTable
	percents := hasWhole && sheet.PercentLabel != ""
	for index, table := range sheet.Tables {
		page.Blocks = append(page.Blocks, Heading(table.Title, 4), pdfTable(table), AmountLine("الاجمالي", totals[index], true))
		if table.AgreedAmount != 0 {
			inside := fixed.Number(0)
			for _, row := range table.Rows {
				if row.CountsAgainstAgreement {
					inside += numberOf(row.Amount)
				}
			}
			agreed := numberOf(table.AgreedAmount)
			page.Blocks = append(page.Blocks, AmountLine("المتبقي", agreed-inside, true), AmountLine("مبلغ الاتفاق", agreed, true))
		}
		if percents {
			page.Blocks = append(page.Blocks, Line(sheet.PercentLabel, percent(totals[index], whole), true))
		}
	}
	if hasWhole {
		page.Blocks = append(page.Blocks, AmountLine("اجمالي الكل", whole, true))
	}
	if percents {
		page.Blocks = append(page.Blocks, Line("نسبة الكل", percent(whole, whole), true))
	}
	return page
}

// pdfTable is a table's header and rows, each under its number (AC-54.6), the first column the widest and the amount the last.
func pdfTable(table Table) Block {
	block := Block{Kind: "table", Gap: 2, Widths: []TableWidth{{Points: pdfNumberWidth}, {Shares: 4}}, Header: []string{table.NumberHeader}, Rows: []PdfRow{}}
	for _, column := range table.Columns {
		block.Widths = append(block.Widths, TableWidth{Shares: 2})
		block.Header = append(block.Header, column.Header)
	}
	block.Header = append(block.Header, table.Amount.Header)
	for _, row := range table.Rows {
		line := PdfRow{Cells: []PdfCell{{Text: textOf(Centred, row.Number)}}, IsNew: row.IsNew}
		for index, column := range table.Columns {
			line.Cells = append(line.Cells, PdfCell{Text: textOf(column.Kind, row.Cells[index])})
		}
		line.Cells = append(line.Cells, PdfCell{Text: Money(numberOf(row.Amount))})
		block.Rows = append(block.Rows, line)
	}
	return block
}

// textOf is a cell's value as the workbook shows it: a day as the statement writes one, a price as money, anything else as it is.
func textOf(kind CellKind, value any) string {
	switch typed := value.(type) {
	case nil:
		return ""
	case time.Time:
		return typed.Format("02.01.2006")
	case float64:
		if kind == Price {
			return Money(numberOf(typed))
		}
		return strconv.FormatFloat(typed, 'f', -1, 64)
	}
	return fmt.Sprint(value)
}

// percent is part of whole as the workbook's 0.00% writes it, rounded half away from zero; 0.00% of nothing.
func percent(part, whole fixed.Number) string {
	if whole == 0 {
		return "0.00%"
	}
	return strconv.FormatFloat(math.Round(float64(part)*10000/float64(whole))/100, 'f', 2, 64) + "%"
}

// numberOf is a workbook amount as the fixed number it was.
func numberOf(amount float64) fixed.Number { return fixed.Number(math.Round(amount * 10000)) }

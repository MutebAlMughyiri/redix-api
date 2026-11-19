// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

// Package statement is the statement workbook (FR-46, redix-api #129): how it looks, as the project's own statement does, and the one layout of
// its sheets, which the periodic report, an item's report and the contracts report share (StatementStyle, StatementWriter, StatementBook,
// ChoiceLists). A sheet is built cell by cell as ClosedXML built it - a value, a formula, a look added to - and written out once, each cell in
// the one style its looks add up to.
package statement

import (
	"bytes"
	"fmt"
	"slices"
	"strings"
	"time"
	"unicode/utf8"

	"github.com/xuri/excelize/v2"
)

// The statement's colours: the grey of header rows and totals, the green of cash a person holds, of the accounts' total and of a row new since
// the last report (BR-26), and the grey text of a line kept for reference, the sales and what leaves them out.
const (
	Grey  = "D9D9D9"
	Green = "B6D7A8"
	Muted = "B7B7B7"
)

// How a number and a date are written in a workbook (ReportFormats, NFR-13): an amount with separators, two decimals and a negative in brackets;
// a date of the periodic report, and of the VAT statement as the accountant's file has it (FR-47).
const (
	MoneyFormat   = "#,##0.00;(#,##0.00)"
	DateFormat    = "dd.mm.yyyy"
	VatDateFormat = "yyyy-mm-dd"
	percentFormat = "0.00%"
)

const (
	font     = "Arial"
	fontSize = 10
	maxWidth = 45
	padding  = 1.5
)

// Book is one statement workbook and what its sheets share (StatementBook): the names taken, the hidden «القوائم» its list cells read, the footer
// every sheet prints, and الخلاصة, which each sheet links back to when the workbook has one.
type Book struct {
	sheets      []*Worksheet
	taken       map[string]bool
	Lists       *ChoiceLists
	Footer      string
	SummaryName string
	plain       bool
}

// NewBook is a workbook whose sheets print footer at their foot, right, and link back to summaryName, "" for a workbook with no summary.
func NewBook(footer, summaryName string) *Book {
	book := &Book{taken: map[string]bool{}, Footer: footer, SummaryName: summaryName}
	book.Lists = newChoiceLists(book)
	return book
}

// NewWorkbook is a workbook in Excel's own look rather than the statement's: Calibri 11, no page setup and no footer, as the VAT statement's
// workbook for the accountant keeps it (the owner, 17 Nov 2026, #76 row 267).
func NewWorkbook() *Book {
	book := NewBook("", "")
	book.plain = true
	return book
}

// SheetName is a name Excel accepts for a sheet, not yet taken in the book (ReportFormats.SheetName): no forbidden character, at most 28 letters
// and a copy's number, taken now.
func (b *Book) SheetName(name string) string {
	clean := strings.TrimSpace(strings.Map(func(letter rune) rune {
		if strings.ContainsRune(`[]:*?/\`, letter) {
			return ' '
		}
		return letter
	}, name))
	if clean == "" {
		clean = "-"
	}
	if utf8.RuneCountInString(clean) > 28 {
		clean = string([]rune(clean)[:28])
	}
	unique := clean
	for number := 2; b.taken[unique]; number++ {
		unique = fmt.Sprintf("%s %d", clean, number)
	}
	b.taken[unique] = true
	return unique
}

// AddSheet adds a sheet named name, unique in the book: right to left, A4 portrait one page wide, its columns widths characters wide.
func (b *Book) AddSheet(name string, widths ...float64) *Worksheet {
	return b.addNamed(b.SheetName(name), widths...)
}

// addNamed adds a sheet of a name already taken for it.
func (b *Book) addNamed(name string, widths ...float64) *Worksheet {
	sheet := &Worksheet{Name: name, cells: map[[2]int]*Cell{}, widths: map[int]float64{}}
	for column, width := range widths {
		sheet.widths[column+1] = width
	}
	b.sheets = append(b.sheets, sheet)
	return sheet
}

// Bytes is the workbook as a file, its sheets in the order they were added.
func (b *Book) Bytes() ([]byte, error) {
	file := excelize.NewFile()
	defer file.Close()
	styles := map[look]int{}
	for index, sheet := range b.sheets {
		var err error
		if index == 0 {
			err = file.SetSheetName("Sheet1", sheet.Name)
		} else {
			_, err = file.NewSheet(sheet.Name)
		}
		if err == nil {
			err = sheet.writeTo(file, styles, b.plain)
		}
		if err != nil {
			return nil, err
		}
	}
	var written bytes.Buffer
	_, err := file.WriteTo(&written)
	return written.Bytes(), err
}

// Reference is the address of cell on the sheet sheetName, as another sheet's formula, a validation or a link names it.
func Reference(sheetName, cell string) string {
	return "'" + strings.ReplaceAll(sheetName, "'", "''") + "'!" + cell
}

// Worksheet is one sheet of the workbook as it is built.
type Worksheet struct {
	Name        string
	cells       map[[2]int]*Cell
	merges      [][4]int
	validations []*excelize.DataValidation
	widths      map[int]float64
	footer      string
	landscape   bool
	printArea   [4]int
	hidden      bool
}

// Cell is the cell on row and column, counted from 1.
func (s *Worksheet) Cell(row, column int) *Cell {
	key := [2]int{row, column}
	if s.cells[key] == nil {
		s.cells[key] = &Cell{Row: row, Column: column}
	}
	return s.cells[key]
}

// Range is the cells from row1, column1 to row2, column2.
func (s *Worksheet) Range(row1, column1, row2, column2 int) Range {
	return Range{sheet: s, top: row1, left: column1, bottom: row2, right: column2}
}

// Print says how the sheet prints (StatementStyle.Print, redix-api #129): A4, one page wide and as many tall as it needs, and at its foot the
// sheet's name on the left, the page of the pages in the centre, and footer, the project and the report's number, on the right.
func (s *Worksheet) Print(footer string) { s.footer = footer }

// Landscape prints the sheet across the page.
func (s *Worksheet) Landscape() { s.landscape = true }

// PrintArea prints only the cells from row1, column1 to row2, column2.
func (s *Worksheet) PrintArea(row1, column1, row2, column2 int) {
	s.printArea = [4]int{row1, column1, row2, column2}
}

// Hide hides the sheet.
func (s *Worksheet) Hide() { s.hidden = true }

// Validate makes the cells of the column from row1 to row2 choose from the list at source, an empty cell allowed when allowsBlank.
func (s *Worksheet) Validate(row1, row2, column int, source string, allowsBlank bool) {
	validation := excelize.NewDataValidation(allowsBlank)
	validation.Sqref = Address(row1, column) + ":" + Address(row2, column)
	validation.SetSqrefDropList(source)
	s.validations = append(s.validations, validation)
}

// Fit fits every column to its content, never narrower than it was set and at most 45 characters, past which its text wraps
// (StatementStyle.Fit); rows keep no height of their own, so Excel grows each to its wrapped text.
func (s *Worksheet) Fit() {
	content := map[int]float64{}
	for key, cell := range s.cells {
		if !s.isMerged(key[0], key[1]) {
			content[key[1]] = max(content[key[1]], cell.width())
		}
	}
	for column, width := range content {
		s.widths[column] = min(maxWidth, max(s.widths[column], width+padding))
	}
}

func (s *Worksheet) isMerged(row, column int) bool {
	return slices.ContainsFunc(s.merges, func(m [4]int) bool { return row >= m[0] && row <= m[2] && column >= m[1] && column <= m[3] })
}

func (s *Worksheet) writeTo(file *excelize.File, styles map[look]int, plain bool) error {
	rightToLeft := true
	errs := []error{file.SetSheetView(s.Name, -1, &excelize.ViewOptions{RightToLeft: &rightToLeft})}
	for column, width := range s.widths {
		letter, _ := excelize.ColumnNumberToName(column)
		errs = append(errs, file.SetColWidth(s.Name, letter, letter, width))
	}
	for _, cell := range s.cells {
		errs = append(errs, cell.writeTo(file, s.Name, styles, plain))
	}
	for _, merge := range s.merges {
		errs = append(errs, file.MergeCell(s.Name, Address(merge[0], merge[1]), Address(merge[2], merge[3])))
	}
	for _, validation := range s.validations {
		errs = append(errs, file.AddDataValidation(s.Name, validation))
	}
	size, orientation, wide, tall, fits := 9, "portrait", 1, 0, true
	if s.landscape {
		orientation = "landscape"
	}
	if !plain {
		errs = append(errs, file.SetPageLayout(s.Name, &excelize.PageLayoutOptions{Size: &size, Orientation: &orientation, FitToWidth: &wide, FitToHeight: &tall}),
			file.SetSheetProps(s.Name, &excelize.SheetPropsOptions{FitToPage: &fits}))
	}
	if s.footer != "" {
		errs = append(errs, file.SetHeaderFooter(s.Name, &excelize.HeaderFooterOptions{OddFooter: "&L&A&C&P / &N&R" + strings.ReplaceAll(s.footer, "&", "&&")}))
	}
	if s.printArea != [4]int{} {
		area := Reference(s.Name, absolute(s.printArea[0], s.printArea[1])+":"+absolute(s.printArea[2], s.printArea[3]))
		errs = append(errs, file.SetDefinedName(&excelize.DefinedName{Name: "_xlnm.Print_Area", RefersTo: area, Scope: s.Name}))
	}
	if s.hidden {
		errs = append(errs, file.SetSheetVisible(s.Name, false))
	}
	for _, err := range errs {
		if err != nil {
			return err
		}
	}
	return nil
}

// Address is the A1 address of row and column.
func Address(row, column int) string {
	address, _ := excelize.CoordinatesToCellName(column, row)
	return address
}

// ColumnLetter is the letter of column.
func ColumnLetter(column int) string {
	letter, _ := excelize.ColumnNumberToName(column)
	return letter
}

func absolute(row, column int) string { return fmt.Sprintf("$%s$%d", ColumnLetter(column), row) }

// Cell is one cell: what it holds, how it looks, and what it opens.
type Cell struct {
	Row, Column int
	Value       any
	Formula     string
	look        look
	link        string
	external    bool
}

// Address is the cell's A1 address.
func (c *Cell) Address() string { return Address(c.Row, c.Column) }

// Set writes value: text, a number, a day, or nil for nothing.
func (c *Cell) Set(value any) *Cell {
	c.Value = value
	return c
}

// Centred writes value, centred.
func (c *Cell) Centred(value any) *Cell {
	c.look.horizontal = "center"
	return c.Set(value)
}

// Money writes amount as an amount: separators, two decimals, a negative in brackets, to the left.
func (c *Cell) Money(amount float64) *Cell {
	c.Value = amount
	return c.asMoney()
}

// MoneyFormula writes formula shown as an amount.
func (c *Cell) MoneyFormula(formula string) *Cell {
	c.Formula = formula
	return c.asMoney()
}

// PercentFormula writes formula shown as a percentage with two decimals, to the left as an amount is.
func (c *Cell) PercentFormula(formula string) *Cell {
	c.Formula, c.look.format, c.look.horizontal = formula, percentFormat, "left"
	return c
}

// Date writes day as the statement does, centred.
func (c *Cell) Date(day time.Time) *Cell {
	c.Value, c.look.format, c.look.horizontal = day, DateFormat, "center"
	return c
}

// Format shows the cell's value in format.
func (c *Cell) Format(format string) *Cell {
	c.look.format = format
	return c
}

// Align places the cell's content: "center" or "left".
func (c *Cell) Align(horizontal string) *Cell {
	c.look.horizontal = horizontal
	return c
}

// LinkTo makes the cell open the sheet sheetName: a link to a place in the workbook, never an address (redix-api #129).
func (c *Cell) LinkTo(sheetName string) *Cell {
	c.link, c.external = Reference(sheetName, "A1"), false
	return c
}

// Opens makes the cell open link, an address outside the workbook, such as a file's signed link.
func (c *Cell) Opens(link string) *Cell {
	c.link, c.external = link, true
	return c
}

func (c *Cell) asMoney() *Cell {
	c.look.format, c.look.horizontal = MoneyFormat, "left"
	return c
}

// width is how many characters the cell's content takes, its longest line, as a fit reads it.
func (c *Cell) width() float64 {
	var text string
	switch value := c.Value.(type) {
	case nil:
		return 0
	case string:
		text = value
	case time.Time:
		text = "00.00.0000"
	case float64:
		text = fmt.Sprintf("%.2f", value)
		if c.look.format == MoneyFormat {
			text = moneyText(value)
		}
	default:
		text = fmt.Sprint(value)
	}
	widest := 0
	for line := range strings.SplitSeq(text, "\n") {
		widest = max(widest, utf8.RuneCountInString(line))
	}
	return float64(widest)
}

func (c *Cell) writeTo(file *excelize.File, sheet string, styles map[look]int, plain bool) error {
	address := c.Address()
	var err error
	switch {
	case c.Formula != "":
		err = file.SetCellFormula(sheet, address, c.Formula)
	case c.Value != nil:
		err = file.SetCellValue(sheet, address, c.Value)
	}
	if err != nil {
		return err
	}
	if c.link != "" {
		kind := "Location"
		if c.external {
			kind = "External"
		}
		if err := file.SetCellHyperLink(sheet, address, c.link, kind); err != nil {
			return err
		}
	}
	style, known := styles[c.look]
	if !known {
		if style, err = file.NewStyle(c.look.style(plain)); err != nil {
			return err
		}
		styles[c.look] = style
	}
	return file.SetCellStyle(sheet, address, address, style)
}

// look is everything a cell's style adds up to.
type look struct {
	fill, color, horizontal, format string
	bold, wrap                      bool
	size                            float64
	left, top, right, bottom        int
}

// The lines of a table: a hair line, and the medium line round a title.
const (
	hair   = 7
	medium = 2
)

// style is the look as one style: in the statement's font, centred upright, or - plain - in Excel's own, Calibri 11 at the cell's foot.
func (l look) style(plain bool) *excelize.Style {
	family, size, vertical := font, float64(fontSize), "center"
	if plain {
		family, size, vertical = "Calibri", 11, ""
	}
	if l.size > 0 {
		size = l.size
	}
	style := &excelize.Style{Font: &excelize.Font{Family: family, Size: size, Bold: l.bold, Color: l.color},
		Alignment: &excelize.Alignment{Horizontal: l.horizontal, Vertical: vertical, WrapText: l.wrap}}
	if l.fill != "" {
		style.Fill = excelize.Fill{Type: "pattern", Pattern: 1, Color: []string{l.fill}}
	}
	if l.format != "" {
		format := l.format
		style.CustomNumFmt = &format
	}
	for side, line := range map[string]int{"left": l.left, "top": l.top, "right": l.right, "bottom": l.bottom} {
		if line != 0 {
			style.Border = append(style.Border, excelize.Border{Type: side, Color: "000000", Style: line})
		}
	}
	return style
}

// Range is a block of cells whose looks change together.
type Range struct {
	sheet                    *Worksheet
	top, left, bottom, right int
}

// First is the range's first cell.
func (r Range) First() *Cell { return r.sheet.Cell(r.top, r.left) }

// Merge makes the range one cell.
func (r Range) Merge() Range {
	if r.top != r.bottom || r.left != r.right {
		r.sheet.merges = append(r.sheet.merges, [4]int{r.top, r.left, r.bottom, r.right})
	}
	return r
}

func (r Range) each(change func(cell *Cell, row, column int)) Range {
	for row := r.top; row <= r.bottom; row++ {
		for column := r.left; column <= r.right; column++ {
			change(r.sheet.Cell(row, column), row, column)
		}
	}
	return r
}

// outline draws line round the range.
func (r Range) outline(line int) Range {
	return r.each(func(cell *Cell, row, column int) {
		if row == r.top {
			cell.look.top = line
		}
		if row == r.bottom {
			cell.look.bottom = line
		}
		if column == r.left {
			cell.look.left = line
		}
		if column == r.right {
			cell.look.right = line
		}
	})
}

// Boxed draws a medium line round the range.
func (r Range) Boxed() Range { return r.outline(medium) }

// Fill paints the range colour.
func (r Range) Fill(colour string) Range {
	return r.each(func(cell *Cell, _, _ int) { cell.look.fill = colour })
}

// Bold makes the range's text bold.
func (r Range) Bold() Range { return r.each(func(cell *Cell, _, _ int) { cell.look.bold = true }) }

// Size sets the range's text size.
func (r Range) Size(size float64) Range {
	return r.each(func(cell *Cell, _, _ int) { cell.look.size = size })
}

// Centre centres the range's content.
func (r Range) Centre() Range {
	return r.each(func(cell *Cell, _, _ int) { cell.look.horizontal = "center" })
}

// Wrap wraps the range's text.
func (r Range) Wrap() Range { return r.each(func(cell *Cell, _, _ int) { cell.look.wrap = true }) }

// UnderlineHair draws a hair line under the range.
func (r Range) UnderlineHair() Range {
	return r.each(func(cell *Cell, row, _ int) {
		if row == r.bottom {
			cell.look.bottom = hair
		}
	})
}

// AsTitle is a table's title row: grey, a medium line round it, centred.
func (r Range) AsTitle() Range { return r.Fill(Grey).outline(medium).Centre() }

// AsHeader is a header row: grey, a hair line under it, centred.
func (r Range) AsHeader() Range {
	return r.Fill(Grey).each(func(cell *Cell, _, _ int) { cell.look.bottom, cell.look.horizontal = hair, "center" })
}

// AsBody is lines of a table: hair lines all round, text wrapping where its column is full.
func (r Range) AsBody() Range {
	return r.each(func(cell *Cell, _, _ int) {
		cell.look.wrap, cell.look.left, cell.look.top, cell.look.right, cell.look.bottom = true, hair, hair, hair, hair
	})
}

// AsTotal is a total: bold on grey.
func (r Range) AsTotal() Range { return r.Bold().Fill(Grey) }

// AsMutedTotal is a total kept for reference: bold grey text on grey.
func (r Range) AsMutedTotal() Range {
	return r.AsTotal().each(func(cell *Cell, _, _ int) { cell.look.color = Muted })
}

// AsNew marks a row new since the last report: green (BR-26).
func (r Range) AsNew() Range { return r.Fill(Green) }

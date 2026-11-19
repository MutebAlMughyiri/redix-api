// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package statement

import (
	"fmt"
	"slices"
	"strings"
	"time"
)

// Sheet is what a statement sheet other than the summary holds (StatementSheet, FR-46): its tables one under another, and the label of each
// table's percentage, "" when it calculates none. When it shows only the tables its reader may see - رأس المال's partners (BR-31, redix-api
// #131 D1) - its اجمالي الكل is Whole, what every table comes to, not their sum.
type Sheet struct {
	Name            string
	Tables          []Table
	PercentLabel    string
	ShowsEveryTable bool
	Whole           float64
}

// Table is one table of a sheet: its title, its columns, its amount, its rows, and what was agreed for it; NumberHeader heads its first column,
// each row's Number, «رقم القيد» on a table of entries (AC-54.6).
type Table struct {
	Title        string
	Columns      []Column
	Amount       Column
	Rows         []Row
	AgreedAmount float64
	NumberHeader string
}

// CellKind is how a column's cells are written: as they are, centred, as a day, or as an amount centred, the price of one.
type CellKind int

const (
	Text CellKind = iota
	Centred
	Day
	Price
)

// Column is a column of a table: its header, how its cells are written, the choices a list's cells take (redix-api #129), whether a cell may be
// left empty - unless entry requires the column (the owner, 13 Nov 2026) - and whether it is «المرفق», whose cell opens its row's file.
type Column struct {
	Header      string
	Kind        CellKind
	Choices     []Choice
	AllowsBlank bool
	IsFile      bool
}

// Row is a row of a table; Number is nil for none, and each of Cells nil when nothing was given. FileLink is the signed link to its entry's own
// file, "" for none (the owner, 16 Nov 2026).
type Row struct {
	Number                 any
	Cells                  []any
	Amount                 float64
	IsNew                  bool
	CountsAgainstAgreement bool
	FileLink               string
}

// Placed is a sheet as its workbook placed it: its name, which a link opens, and the address of its last total, which the summary's line reads.
type Placed struct {
	Name, Total string
}

// NotPlaced is no sheet: what an item without one reads, its total nothing.
var NotPlaced = Placed{Total: "0"}

const (
	numberWidth = 4.25
	// landscapeWidth is how many columns wide, its number and its amount counted, a table prints across the page (redix-api #129).
	landscapeWidth = 8
	backToSummary  = "← الخلاصة"
)

// Add writes content as a sheet of the book, its name unique among the book's, and answers its name and the address of its last total as another
// sheet's formula names it: اجمالي الكل, or its one table's الاجمالي (StatementWriter.Add). Tables go one under another with a blank row between;
// each has a title, a header and its numbered rows, then its lines: الاجمالي; المتبقي and مبلغ الاتفاق when an agreed amount was filled in; its
// percentage when the sheet calculates one. With more than one table come اجمالي الكل and, with percentages, نسبة الكل. Every amount sits in the
// last column; each list column's cells read their choices from the book's «القوائم»; the sheet prints its tables alone, across the page from
// eight columns, «← الخلاصة» two columns after them, outside what prints.
func (b *Book) Add(content Sheet) Placed {
	widest := 0
	for _, table := range content.Tables {
		widest = max(widest, len(table.Columns))
	}
	sheet := b.AddSheet(content.Name, numberWidth)
	last := widest + 2
	hasWhole := len(content.Tables) > 1 || !content.ShowsEveryTable
	var totals []string
	var percents []*Cell
	row := 1
	for _, table := range content.Tables {
		total, first := b.table(sheet, &row, last, table)
		if table.AgreedAmount != 0 {
			agreed := line(sheet, row+1, last, "مبلغ الاتفاق").Money(table.AgreedAmount).Address()
			line(sheet, row, last, "المتبقي").MoneyFormula(agreed + "-" + insideOf(table, total, first, last))
			row += 2
		}
		if hasWhole && content.PercentLabel != "" {
			percents = append(percents, line(sheet, row, last, content.PercentLabel))
			row++
		}
		totals = append(totals, total)
		row++
	}
	lastTotal := totals[0]
	if hasWhole {
		whole := line(sheet, row, last, "اجمالي الكل")
		if content.ShowsEveryTable {
			whole.MoneyFormula(strings.Join(totals, "+"))
		} else {
			whole.Money(content.Whole)
		}
		lastTotal = whole.Address()
		for index, percent := range percents {
			percent.PercentFormula(fmt.Sprintf("IF(%s=0,0,%s/%s)", lastTotal, totals[index], lastTotal))
		}
		if len(percents) > 0 {
			row++
			var addresses []string
			for _, percent := range percents {
				addresses = append(addresses, percent.Address())
			}
			line(sheet, row, last, "نسبة الكل").PercentFormula(strings.Join(addresses, "+"))
		}
	} else {
		row -= 2
	}
	if b.SummaryName != "" {
		sheet.Cell(1, last+2).Set(backToSummary).LinkTo(b.SummaryName)
	}
	sheet.Fit()
	sheet.Print(b.Footer)
	if last >= landscapeWidth {
		sheet.Landscape()
	}
	sheet.PrintArea(1, 1, row, last)
	return Placed{Name: sheet.Name, Total: Reference(sheet.Name, lastTotal)}
}

// table writes the table's title, header, rows and الاجمالي from row, leaving row on the row after; it answers the address of الاجمالي and the row
// of the first line. A list column's cells take a validation reading its block of the book's lists, an empty cell allowed where entry allows one.
func (b *Book) table(sheet *Worksheet, row *int, last int, table Table) (string, int) {
	sheet.Range(*row, 1, *row, last).Merge().AsTitle().First().Set(table.Title)
	*row++
	stretch := last - 2 - len(table.Columns)
	sheet.Range(*row, 1, *row, last).AsHeader()
	sheet.Cell(*row, 1).Set(table.NumberHeader)
	for column, header := range table.Columns {
		span(sheet, *row, column, stretch).Set(header.Header)
	}
	sheet.Cell(*row, last).Set(table.Amount.Header)
	*row++
	first := *row
	for _, entry := range table.Rows {
		sheet.Cell(*row, 1).Centred(entry.Number)
		for column, kind := range table.Columns {
			cell := span(sheet, *row, column, stretch)
			write(cell, kind.Kind, entry.Cells[column])
			if kind.IsFile && entry.FileLink != "" {
				cell.Opens(entry.FileLink)
			}
		}
		sheet.Cell(*row, last).Money(entry.Amount)
		body := sheet.Range(*row, 1, *row, last).AsBody()
		if entry.IsNew {
			body.AsNew()
		}
		*row++
	}
	for column, listed := range table.Columns {
		if len(listed.Choices) > 0 && *row > first {
			place := span(sheet, first, column, stretch).Column
			sheet.Validate(first, *row-1, place, b.Lists.RangeOf(listed.Header, listed.Choices), listed.AllowsBlank)
		}
	}
	letter := ColumnLetter(last)
	sum := "0"
	if *row > first {
		sum = fmt.Sprintf("SUM(%s%d:%s%d)", letter, first, letter, *row-1)
	}
	total := line(sheet, *row, last, "الاجمالي").MoneyFormula(sum).Address()
	*row++
	return total, first
}

// insideOf is the payments that count against the agreed amount: the total less those outside the contract (BR-04), whose rows start at first.
func insideOf(table Table, total string, first, last int) string {
	var outside []string
	for index, entry := range table.Rows {
		if !entry.CountsAgainstAgreement {
			outside = append(outside, fmt.Sprintf("%s%d", ColumnLetter(last), first+index))
		}
	}
	if len(outside) == 0 {
		return total
	}
	return "(" + total + "-(" + strings.Join(outside, "+") + "))"
}

// span is the cell of column on row, after the number; the first column takes stretch more, so the amount stays in the last.
func span(sheet *Worksheet, row, column, stretch int) *Cell {
	from, to := column+2+stretch, column+2+stretch
	if column == 0 {
		from, to = 2, 2+stretch
	}
	if to > from {
		return sheet.Range(row, from, row, to).Merge().First()
	}
	return sheet.Cell(row, from)
}

func write(cell *Cell, kind CellKind, value any) {
	if value == nil {
		return
	}
	if day, isDay := value.(time.Time); isDay && kind == Day {
		cell.Date(day)
		return
	}
	cell.Set(value)
	switch kind {
	case Centred:
		cell.Align("center")
	case Price:
		cell.Format(MoneyFormat).Align("center")
	case Text, Day:
	}
}

// line is a line under a table: its label across every column but the last, grey and bold; it answers the cell its figure goes in.
func line(sheet *Worksheet, row, last int, label string) *Cell {
	sheet.Range(row, 1, row, last-1).Merge().First().Set(label)
	sheet.Range(row, 1, row, last).AsTotal()
	return sheet.Cell(row, last)
}

// Choice is one of a list's choices with the sign it gives the amount, as a workbook writes it: "+", "−", or "" for none.
type Choice struct {
	Name, Sign string
}

// ChoiceLists is the sheet «القوائم», hidden and last in a workbook (redix-api #129): every list a table's cells choose from, a block each, titled
// with the list's column, its choices under «الاختيار» with their signs under «الإشارة». Each list cell's validation reads its block, so the
// choices live in one place, as a column's definition keeps them.
type ChoiceLists struct {
	sheetName string
	blocks    []choiceBlock
	nextRow   int
}

type choiceBlock struct {
	title   string
	choices []Choice
	row     int
}

// newChoiceLists is the lists of the book; the sheet's own name is taken now, so every validation can name it before it is written.
func newChoiceLists(book *Book) *ChoiceLists {
	return &ChoiceLists{sheetName: book.SheetName("القوائم"), nextRow: 1}
}

// RangeOf is the cells of the block holding choices, the list of the column title, as a validation names them; one list shares one block.
func (l *ChoiceLists) RangeOf(title string, choices []Choice) string {
	index := slices.IndexFunc(l.blocks, func(block choiceBlock) bool { return block.title == title && slices.Equal(block.choices, choices) })
	if index < 0 {
		l.blocks = append(l.blocks, choiceBlock{title: title, choices: choices, row: l.nextRow})
		l.nextRow += len(choices) + 3
		index = len(l.blocks) - 1
	}
	block := l.blocks[index]
	return Reference(l.sheetName, fmt.Sprintf("$A$%d:$A$%d", block.row+2, block.row+1+len(choices)))
}

// AddTo adds the sheet to the book, hidden, after every other, when any table holds a list; nothing otherwise.
func (l *ChoiceLists) AddTo(book *Book) {
	if len(l.blocks) == 0 {
		return
	}
	sheet := book.addNamed(l.sheetName, 18, 10)
	for _, block := range l.blocks {
		sheet.Range(block.row, 1, block.row, 2).Merge().AsTitle().First().Set(block.title)
		sheet.Range(block.row+1, 1, block.row+1, 2).AsHeader()
		sheet.Cell(block.row+1, 1).Set("الاختيار")
		sheet.Cell(block.row+1, 2).Set("الإشارة")
		for place, choice := range block.choices {
			sheet.Cell(block.row+2+place, 1).Set(choice.Name)
			sheet.Cell(block.row+2+place, 2).Centred(choice.Sign)
		}
		sheet.Range(block.row+2, 1, block.row+1+len(block.choices), 2).AsBody()
	}
	sheet.Print(book.Footer)
	sheet.Hide()
}

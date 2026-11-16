// A report PDF (UC-054, FR-45, #140 D2): A4 portrait, right to left, in IBM Plex Sans Arabic, numbered at its foot. The API works out every
// word and figure and writes them to data.json; this only lays them out, a page at a time.
#let data = json("data.json")
#set page(paper: "a4", margin: 1.2cm, footer: context align(center, counter(page).display()))
#set text(font: "IBM Plex Sans Arabic", size: 9pt, lang: "ar")
#set block(spacing: 5pt)
#let header-grey = rgb("#eeeeee")
#let line-grey = rgb("#e0e0e0")
#let new-green = rgb("#c8e6c9")
#let weight(strong) = if strong { "bold" } else { "regular" }
#let width(column) = if "pt" in column { column.pt * 1pt } else { column.fr * 1fr }

#for (index, page) in data.pages.enumerate() {
  if index > 0 { pagebreak() }
  text(size: page.titleSize * 1pt, weight: "bold", page.title)
  for part in page.blocks {
    if part.gap > 0 { v(part.gap * 1pt) }
    if part.kind == "heading" {
      block(text(weight: "bold", part.text))
    } else if part.kind == "line" {
      grid(columns: (1fr, 120pt), text(weight: weight(part.strong), part.text), align(left, text(weight: weight(part.strong), part.figure)))
    } else if part.kind == "table" {
      let rows = part.rows
      table(
        columns: part.widths.map(width),
        inset: 3pt,
        stroke: (x, y) => if y == 0 { none } else { (bottom: 0.5pt + line-grey) },
        fill: (x, y) => if y == 0 { header-grey } else if rows.at(y - 1).isNew { new-green } else { white },
        table.header(..part.header.map(title => text(weight: "bold", title))),
        ..rows.map(row => row.cells.map(cell => text(weight: weight(cell.strong), cell.text))).flatten(),
      )
    }
  }
}

// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package reports

import (
	"encoding/json"
	"strings"
	"testing"
)

// keptByDotNet is a report as .NET kept it in PeriodicReports.Snapshot: its kinds written as numbers - an item type, an account type, a column's
// data type, a choice's sign - and its figures in the scales they were computed with.
const keptByDotNet = `{"projectName":"p","number":2,"issuedAt":"2026-11-16T13:32:44.03815Z","previousExpenses":100.0000,"expenses":140.0000,` +
	`"capital":0,"vatRefunds":0,"sales":0,"otherRevenue":0,"surplus":-140.0000,"surplusWithoutSales":-140.0000,` +
	`"categories":[{"id":"01a1e48f-2bd8-71de-8fdf-642e5c13804e","name":"c","total":140.0000,"items":[{"id":"01a1e48f-2bd8-7228-8e25-1d181a1e4551","name":"i","total":140.0000,"type":2}]}],` +
	`"accounts":[{"name":"a","type":1,"balance":12.50}],"accountsTotal":12.50,"reconciliationDifference":152.5000,"pendingCount":0,"pendingAmount":0,` +
	`"capitalTotal":0,"partners":[{"name":"s","id":"01a1e48f-53c6-7a6d-b736-6cec2bd34406","balance":1000.0000,"sharePercent":100}],` +
	`"items":[{"itemId":"01a1e48f-2bd8-7228-8e25-1d181a1e4551","itemName":"i","categoryName":"c","total":140.0000,"subgroups":[{"id":"01a1e48f-2bd8-7fbd-bba2-546c9a823b82",` +
	`"name":"","total":140.0000,"rows":[{"entryId":"01a1e48f-2cba-7804-a3d3-861cc3963894","date":"2026-11-16","amount":100.0000,"isOutsideContract":false,` +
	`"values":["x"],"isNew":false,"number":1}],"sortOrder":0,"columns":[{"key":"entryType","name":"t","dataType":4,"isRequired":false,` +
	`"choices":[{"name":"r","sign":2}]}],"amountName":"n"}],"contracts":[],"showsTablePercentages":false,"type":2,"showsEveryTable":true}],` +
	`"revenueCategories":[],"pendingRevenueCount":0,"pendingRevenueAmount":0,"expensesDifference":40.0000,"revenue":0,"revenueWithoutSales":0}`

func TestBR26_AReportDotNetKeptIsAnsweredAsItWasIssued(t *testing.T) {
	var report snapshot
	if err := json.Unmarshal([]byte(keptByDotNet), &report); err != nil {
		t.Fatal(err)
	}
	answered, err := json.Marshal(report)
	if err != nil {
		t.Fatal(err)
	}
	named := strings.NewReplacer(`"type":2`, `"type":"both"`, `"type":1`, `"type":"cashCustody"`, `"dataType":4`, `"dataType":"list"`,
		`"sign":2`, `"sign":"minus"`).Replace(keptByDotNet)
	if string(answered) != named {
		t.Errorf("the kept report was answered as\n%s\nwant\n%s", answered, named)
	}
}

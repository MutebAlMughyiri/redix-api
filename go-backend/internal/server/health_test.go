// Copyright 2026 Tech Details. All rights reserved.
// Use of this file is governed by the LICENSE file at the root of this repository.

package server_test

import (
	"net/http"
	"testing"

	"github.com/MutebAlMughyiri/redix-api/go-backend/internal/testapi"
)

// UC-001: /health answers Healthy while the API reaches its database, and 503 Unhealthy once it cannot.
func TestUC001_HealthSaysWhetherTheDatabaseIsReached(t *testing.T) {
	api := testapi.New(t)
	if answered := api.Send("GET", "/health", ""); answered.Code != http.StatusOK || answered.Body.String() != "Healthy" {
		t.Errorf("reached: %d %q", answered.Code, answered.Body)
	}
	if answered, failed := api.SendFailing(1, "GET", "/health", ""); !failed || answered.Code != http.StatusServiceUnavailable || answered.Body.String() != "Unhealthy" {
		t.Errorf("not reached: %v %d %q", failed, answered.Code, answered.Body)
	}
}

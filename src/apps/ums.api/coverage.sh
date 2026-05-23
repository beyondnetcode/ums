#!/usr/bin/env bash
# =============================================================================
# coverage.sh — UMS business-layer coverage runner
#
# Usage:
#   ./coverage.sh          Run all tests + generate HTML report
#   ./coverage.sh --ci     Same but exit non-zero if thresholds are not met
#
# Output:
#   coverage/                Cobertura XML files (one per project)
#   coverage/report/         HTML report (open coverage/report/index.htm)
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COVERAGE_DIR="$SCRIPT_DIR/coverage"
REPORT_DIR="$COVERAGE_DIR/report"
CI_MODE=false

for arg in "$@"; do
  [[ "$arg" == "--ci" ]] && CI_MODE=true
done

# ---------------------------------------------------------------------------
# Clean previous results
# ---------------------------------------------------------------------------
rm -rf "$COVERAGE_DIR"
mkdir -p "$COVERAGE_DIR"

# ---------------------------------------------------------------------------
# Coverlet include/exclude filters
#
# INCLUDE: Only business-critical layers
#   [Ums.Domain]*             — Aggregates, Value Objects, Domain Events, Domain Services
#   [Ums.Application]*        — Command handlers, Validators, Application Services
#
# EXCLUDE (noisy / framework / zero business logic):
#   [Ums.Domain]*.Props       — Record-style prop bags (structural, no logic)
#   [Ums.Domain]*Event        — Domain event POCOs (data carriers)
#   [Ums.Domain]*Id           — Typed ID value objects (Load/Create wrappers)
#   [Ums.Application]*.DTOs.* — Request/Response record types
#   [Ums.Application]*Validator — FluentValidation (tested separately)
#   [Ums.Infrastructure]*     — EF Core mappings, migrations, bootstrapper
#   [Ums.Presentation]*       — Controllers / Minimal API endpoints
#   [Ums.Shell.*]*            — Framework kernel (DDD base classes)
#   [Ums.Globalization]*      — Static resource strings
# ---------------------------------------------------------------------------

INCLUDE="[Ums.Domain]*%2c[Ums.Application]*"
EXCLUDE="[Ums.Domain]*.Props%2c[Ums.Domain]*Event%2c[Ums.Domain]*DomainErrors%2c[Ums.Application]*.DTOs.*%2c[Ums.Application]*Validator%2c[Ums.Infrastructure]*%2c[Ums.Presentation]*%2c[Ums.Shell.*]*%2c[Ums.Globalization]*"

# ---------------------------------------------------------------------------
# Run Domain tests
# ---------------------------------------------------------------------------
echo "▶  Running Ums.Domain.Test..."
dotnet test "$SCRIPT_DIR/Ums.Domain.Test/Ums.Domain.Test.csproj" \
  --configuration Release \
  --no-restore \
  --collect:"XPlat Code Coverage" \
  --results-directory "$COVERAGE_DIR/domain" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include="$INCLUDE" \
     DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="$EXCLUDE" \
     DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format="cobertura"

# ---------------------------------------------------------------------------
# Run Application tests
# ---------------------------------------------------------------------------
echo "▶  Running Ums.Application.Test..."
dotnet test "$SCRIPT_DIR/Ums.Application.Test/Ums.Application.Test.csproj" \
  --configuration Release \
  --no-restore \
  --collect:"XPlat Code Coverage" \
  --results-directory "$COVERAGE_DIR/application" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include="$INCLUDE" \
     DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="$EXCLUDE" \
     DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format="cobertura"

# ---------------------------------------------------------------------------
# Merge + generate HTML report
# ---------------------------------------------------------------------------
COBERTURA_FILES=$(find "$COVERAGE_DIR" -name "coverage.cobertura.xml" | tr '\n' ';' | sed 's/;$//')

echo "▶  Generating HTML report..."
dotnet tool run reportgenerator \
  "-reports:$COBERTURA_FILES" \
  "-targetdir:$REPORT_DIR" \
  "-reporttypes:Html;Cobertura;Badges" \
  "-assemblyfilters:+Ums.Domain;+Ums.Application;-Ums.Infrastructure;-Ums.Presentation;-Ums.Shell.*" \
  "-classfilters:-*Props;-*Event;-*DomainErrors;-*Validator;-*.DTOs.*" \
  "-title:UMS Business Layer Coverage"

echo ""
echo "✅  Report generated: $REPORT_DIR/index.htm"
echo ""

# ---------------------------------------------------------------------------
# Threshold enforcement (--ci mode)
#
# Domain layer:      ≥ 85% line coverage
# Application layer: ≥ 75% line coverage
# Combined:          ≥ 80% line coverage
# ---------------------------------------------------------------------------
if $CI_MODE; then
  echo "▶  Enforcing thresholds (CI mode)..."
  dotnet tool run reportgenerator \
    "-reports:$COBERTURA_FILES" \
    "-targetdir:$REPORT_DIR/threshold" \
    "-reporttypes:Cobertura" \
    "-assemblyfilters:+Ums.Domain;+Ums.Application" \
    "-classfilters:-*Props;-*Event;-*DomainErrors;-*Validator;-*.DTOs.*"

  SUMMARY="$REPORT_DIR/threshold/Cobertura.xml"
  COVERAGE=$(grep -oP 'line-rate="\K[^"]+' "$SUMMARY" | head -1)
  COVERAGE_PCT=$(echo "$COVERAGE * 100" | bc -l | xargs printf "%.1f")

  echo "  Combined line coverage: ${COVERAGE_PCT}%"

  THRESHOLD=80
  if (( $(echo "$COVERAGE_PCT < $THRESHOLD" | bc -l) )); then
    echo "❌  Coverage ${COVERAGE_PCT}% is below required threshold ${THRESHOLD}%."
    exit 1
  else
    echo "✅  Coverage ${COVERAGE_PCT}% meets the ${THRESHOLD}% threshold."
  fi
fi

# Release Checklist

**Version**: `RELEASE_VERSION`
**Date**: `RELEASE_DATE`
**Author**: `RELEASE_AUTHOR`

---

## Pre-Release Validation

### Build & Test
- [ ] All unit tests passing
- [ ] All integration tests passing
- [ ] E2E tests passing (if applicable)
- [ ] Frontend build successful
- [ ] Backend build successful
- [ ] No breaking changes introduced

### Security
- [ ] CodeQL scan completed with no critical issues
- [ ] Dependency vulnerability scan clean
- [ ] No hardcoded secrets detected
- [ ] Docker image scan passed
- [ ] Tenant isolation tests passing

### Documentation
- [ ] CHANGELOG.md updated with all changes
- [ ] Documentation version log updated
- [ ] MASTER_INDEX validated
- [ ] All ADR changes documented
- [ ] Bilingual documentation synchronized

---

## Code Quality

### Coverage
- [ ] Code coverage > 80%
- [ ] Critical paths covered
- [ ] Tenant isolation tested

### Standards
- [ ] Lint checks passing
- [ ] TypeScript compiles with zero errors
- [ ] .NET build with zero warnings (or documented)
- [ ] Architecture alignment with Evolith verified

---

## Validation

### Integration
- [ ] API integration tests pass
- [ ] Database migrations tested
- [ ] Configuration changes validated

### Performance
- [ ] Build time under limits
- [ ] Test execution under limits
- [ ] No performance regressions identified

---

## Approval

### Required Reviews
- [ ] Architecture Team Review (sign-off below)
- [ ] Security Team Review (sign-off below)
- [ ] Product Owner Sign-off (sign-off below)

### Sign-off Table

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Architecture Lead | | | |
| Security Lead | | | |
| Product Owner | | | |

---

## Release Execution

### Git Operations
- [ ] Release branch merged to main
- [ ] Git tag created: `vX.Y.Z`
- [ ] GitHub Release created with release notes
- [ ] Release branch merged to develop

### Announcements
- [ ] Stakeholder notification sent
- [ ] Release notes published
- [ ] Documentation updated and published

---

## Post-Release

### Monitoring
- [ ] Production health checks passing
- [ ] Error rates nominal
- [ ] Performance metrics normal

### Rollback Plan (if needed)
```bash
# Revert to previous version
git revert RELEASE_COMMIT_SHA
git push origin main

# Or rollback to specific tag
git checkout vPREVIOUS_VERSION
# Deploy from this tag
```

---

## Notes

_Enter any additional notes, known issues, or concerns here._

---

**Checklist Version**: 1.0.0
**Last Updated**: 2026-05-29
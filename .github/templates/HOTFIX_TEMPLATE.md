# Hotfix Template

**Hotfix Branch**: `hotfix/NAME`
**Date**: `YYYY-MM-DD`
**Author**: `AUTHOR`

---

## Issue Summary

_Describe the issue being fixed._

### Severity
- [ ] Critical (Production down)
- [ ] High (Major functionality broken)
- [ ] Medium (Minor issue with workaround)
- [ ] Low (Cosmetic/Non-blocking)

### Impact
_Describe the impact on users and systems._

---

## Root Cause

_Explain the root cause of the issue._

---

## Fix Description

_Describe the fix implemented._

### Changes Made
- `file1.cs`: Description of change
- `file2.ts`: Description of change

---

## Testing

### Validation Steps
- [ ] Issue reproduced
- [ ] Fix applied
- [ ] Issue resolved
- [ ] No regression in related functionality

### Test Results
| Test | Result |
|------|--------|
| Unit Tests | PASS/FAIL |
| Integration Tests | PASS/FAIL |
| Manual Verification | PASS/FAIL |

---

## Security Considerations

- [ ] No new security vulnerabilities introduced
- [ ] Security scan clean
- [ ] No secrets hardcoded

---

## Rollback Plan

### If Issues Detected After Merge

**Option 1: Revert Commit**
```bash
git revert HOTFIX_COMMIT_SHA
git push origin main
```

**Option 2: Rollback to Previous Version**
```bash
git checkout vPREVIOUS_VERSION
# Deploy from this version
```

---

## Approval

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Technical Lead | | | |
| Security Review | | | |
| Product Owner (if critical) | | | |

---

## Notes

_Enter any additional notes._

---

**Template Version**: 1.0.0
**Last Updated**: 2026-05-29
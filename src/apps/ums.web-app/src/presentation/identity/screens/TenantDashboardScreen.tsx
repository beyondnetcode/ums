import React, { useEffect, useRef } from 'react';
import {
  useGetAllTenants,
  useActivateTenant,
  useSuspendTenant
} from '../../../application/identity/hooks/use-tenant';
import { useI18n } from '../../../application/i18n/use-i18n';
import { TenantForm } from '../components/TenantForm';
import { BranchManager } from '../components/BranchManager';
import { M3Card } from '../../shared/components/M3Card';
import { M3Button } from '../../shared/components/M3Button';
import { M3TextField } from '../../shared/components/M3TextField';
import { M3Select } from '../../shared/components/M3Select';
import { M3DataView, SortOption, FilterOption, QueryCriteriaOption } from '../../shared/components/M3DataView';
import { Tenant } from '../../../domain/identity/models/tenant.model';
import { AuthProvider, IdpStrategy } from '../../../domain/identity/models/idp.model';
import { BrandingConfig, DEFAULT_BRANDING } from '../../../domain/identity/models/branding.model';
import { useNotificationStore } from '../../../application/stores/notification.store';
import { isValidPublicUrl, sanitizeInput, sanitizeCode } from '../../../application/utils/security';
import {
  Building2,
  ArrowRight,
  RefreshCw,
  Sliders,
  ShieldAlert,
  CheckCircle2,
  Check,
  Building,
  Info,
  MapPin,
  Key,
  Palette,
  Plus,
  Trash2,
  X,
  Pencil,
  Save
} from 'lucide-react';
import { IconButton, Tooltip } from '../../shared/components/Tooltip';

// ─── Component ──────────────────────────────────────────────────────────────

export const TenantDashboardScreen: React.FC = () => {
  const addNotification = useNotificationStore((s) => s.addNotification);
  const t = useI18n();

  // ── API ────────────────────────────────────────────────────────────────────
  const { data: apiTenants = [], isLoading: isLoadingList } = useGetAllTenants();
  const [localOverrides, setLocalOverrides] = React.useState<Record<string, Partial<Tenant>>>({});

  const knownTenants: Tenant[] = apiTenants.map((tenant) => ({
    ...tenant,
    ...localOverrides[tenant.tenantId],
  }));

  const patchKnownTenants = (updater: (prev: Tenant[]) => Tenant[]) => {
    const next = updater(knownTenants);
    const overrides: Record<string, Partial<Tenant>> = {};
    next.forEach((tenant) => { overrides[tenant.tenantId] = tenant; });
    setLocalOverrides(overrides);
  };

  // ── Selection ──────────────────────────────────────────────────────────────
  const [selectedId, setSelectedId] = React.useState<string>('');
  const [showDiscardDialog, setShowDiscardDialog] = React.useState(false);
  const [pendingNavigationId, setPendingNavigationId] = React.useState<string | null>(null);
  const [activeConsoleTab, setActiveConsoleTab] = React.useState<'branches' | 'providers' | 'branding'>('branches');

  // ── Tenant inline-edit ────────────────────────────────────────────────────
  const [isTenantEditing, setIsTenantEditing] = React.useState(false);
  const [editName, setEditName] = React.useState('');
  const [editCode, setEditCode] = React.useState('');
  const [editCompanyRef, setEditCompanyRef] = React.useState('');
  const [editType, setEditType] = React.useState('');

  // ── Provider data + inline-edit ───────────────────────────────────────────
  const [providersData, setProvidersData] = React.useState<Record<string, AuthProvider[]>>({});
  const [editingProviderId, setEditingProviderId] = React.useState<string | null>(null);
  const [editProvName, setEditProvName] = React.useState('');
  const [editProvCode, setEditProvCode] = React.useState('');
  const [editProvDescription, setEditProvDescription] = React.useState('');
  const [editProvStrategy, setEditProvStrategy] = React.useState<IdpStrategy>('OIDC');

  // ── Provider add form ─────────────────────────────────────────────────────
  const [isAddingProvider, setIsAddingProvider] = React.useState(false);
  const [provName, setProvName] = React.useState('');
  const [provCode, setProvCode] = React.useState('');
  const [provDescription, setProvDescription] = React.useState('');
  const [provStrategy, setProvStrategy] = React.useState<IdpStrategy>('OIDC');

  // ── Branding data + form ──────────────────────────────────────────────────
  const [brandingData, setBrandingData] = React.useState<Record<string, BrandingConfig>>({});
  const [brandHeadline, setBrandHeadline] = React.useState('');
  const [brandSecondary, setBrandSecondary] = React.useState('');
  const [brandButtonLabel, setBrandButtonLabel] = React.useState('');
  const [brandFooter, setBrandFooter] = React.useState('');
  const [brandColor, setBrandColor] = React.useState('#3b5bdb');
  const [brandBackground, setBrandBackground] = React.useState('solid');
  const [brandLogo, setBrandLogo] = React.useState('');
  const [brandLogoFormat, setBrandLogoFormat] = React.useState('png');
  const [brandCustomDomain, setBrandCustomDomain] = React.useState('');
  const [brandMagicLink, setBrandMagicLink] = React.useState(false);

  // ── List controls ──────────────────────────────────────────────────────────
  const [isCreateOpen, setIsCreateOpen] = React.useState(false);
  const [viewMode, setViewMode] = React.useState<'list' | 'thumbnail'>('list');
  const [searchCriteria, setSearchCriteria] = React.useState<string>('name');
  const [searchValue, setSearchValue] = React.useState<string>('');
  const [appliedQuery, setAppliedQuery] = React.useState<{ criteria: string; term: string }>({ criteria: 'name', term: '' });
  const [activeFilter, setActiveFilter] = React.useState<string>('all');
  const [sortBy, setSortBy] = React.useState<string>('name');
  const [sortOrder, setSortOrder] = React.useState<'asc' | 'desc'>('asc');
  const [page, setPage] = React.useState<number>(1);
  const pageSize = 9;

  const activeTenant = knownTenants.find((tenant) => tenant.tenantId === selectedId);
  const activateMutation = useActivateTenant(selectedId);
  const suspendMutation = useSuspendTenant(selectedId);
  const isPendingMutation = activateMutation.isPending || suspendMutation.isPending;
  const activeProviders = providersData[selectedId] || [];
  const hasPendingChanges = isTenantEditing || !!editingProviderId;

  // ── Helpers ───────────────────────────────────────────────────────────────

  /** Synchronously reset all right-panel state when switching tenants. */
  const applyTenantSelection = (id: string) => {
    const b = brandingData[id] ?? DEFAULT_BRANDING;
    setBrandHeadline(b.headlineText);
    setBrandSecondary(b.secondaryText);
    setBrandButtonLabel(b.primaryButtonLabel);
    setBrandFooter(b.footerText);
    setBrandColor(b.primaryColor);
    setBrandBackground(b.backgroundStyle);
    setBrandLogo(b.logo);
    setBrandLogoFormat(b.logoFormat);
    setBrandCustomDomain(b.customDomain);
    setBrandMagicLink(b.magicLinkFallbackEnabled);
    setIsAddingProvider(false);
    setProvName(''); setProvCode(''); setProvDescription('');
    setIsTenantEditing(false);
    setEditingProviderId(null);
    setActiveConsoleTab('branches');
    setSelectedId(id);
  };

  const handleSelectTenant = (id: string) => {
    if (id === selectedId) return;
    if (hasPendingChanges) {
      setPendingNavigationId(id);
      setShowDiscardDialog(true);
      return;
    }
    applyTenantSelection(id);
  };

  const confirmDiscard = () => {
    if (pendingNavigationId) applyTenantSelection(pendingNavigationId);
    setPendingNavigationId(null);
    setShowDiscardDialog(false);
  };

  const applyTenantSelectionRef = useRef(applyTenantSelection);
  applyTenantSelectionRef.current = applyTenantSelection;

  useEffect(() => {
    if (!selectedId && apiTenants.length > 0) {
      applyTenantSelectionRef.current(apiTenants[0].tenantId);
    }
  }, [apiTenants, selectedId]);

  // ── Tenant edit ───────────────────────────────────────────────────────────

  const openTenantEdit = () => {
    if (!activeTenant) return;
    setEditName(activeTenant.name);
    setEditCode(activeTenant.code);
    setEditCompanyRef(activeTenant.companyReference || '');
    setEditType(activeTenant.type);
    setIsTenantEditing(true);
  };

  const saveTenantEdit = () => {
    const sanitizedName = sanitizeInput(editName);
    const sanitizedCode = sanitizeCode(editCode);
    const sanitizedRef = sanitizeInput(editCompanyRef);
    if (!sanitizedName.trim() || !sanitizedCode.trim()) return;
    patchKnownTenants((prev) =>
      prev.map((tenant) =>
        tenant.tenantId === selectedId
          ? { ...tenant, name: sanitizedName.trim(), code: sanitizedCode.trim(), companyReference: sanitizedRef.trim(), type: editType }
          : tenant
      )
    );
    addNotification({ title: t.notifTenantUpdated, message: t.notifTenantUpdatedMsg(sanitizedName.trim()), type: 'success' });
    setIsTenantEditing(false);
  };

  const handleToggleStatus = (newStatus: 'Active' | 'Suspended') => {
    const previousStatus = activeTenant?.status;
    patchKnownTenants((prev) =>
      prev.map((tenant) => (tenant.tenantId === selectedId ? { ...tenant, status: newStatus } : tenant))
    );
    addNotification({
      title: newStatus === 'Active' ? t.notifActivated : t.notifSuspended,
      message: t.notifStatusSetTo(newStatus),
      type: newStatus === 'Active' ? 'success' : 'warning',
    });
    const mutation = newStatus === 'Active' ? activateMutation : suspendMutation;
    mutation.mutate(undefined, {
      onError: () => {
        if (previousStatus) {
          patchKnownTenants((prev) =>
            prev.map((tenant) =>
              tenant.tenantId === selectedId ? { ...tenant, status: previousStatus } : tenant
            )
          );
        }
        addNotification({
          title: t.notifStatusChangeFailed,
          message: t.notifStatusChangeFailedMsg(newStatus),
          type: 'error',
        });
      },
    });
  };

  // ── Provider handlers ─────────────────────────────────────────────────────

  const openProviderEdit = (p: AuthProvider) => {
    setEditingProviderId(p.id);
    setEditProvName(p.name);
    setEditProvCode(p.code);
    setEditProvDescription(p.description);
    setEditProvStrategy(p.strategy);
  };

  const saveProviderEdit = () => {
    const sanitizedName = sanitizeInput(editProvName);
    const sanitizedCode = sanitizeCode(editProvCode);
    const sanitizedDesc = sanitizeInput(editProvDescription);
    if (!sanitizedName.trim() || !editingProviderId) return;
    setProvidersData((prev) => ({
      ...prev,
      [selectedId]: (prev[selectedId] || []).map((p) =>
        p.id === editingProviderId
          ? { ...p, name: sanitizedName.trim(), code: sanitizedCode.trim(), description: sanitizedDesc.trim(), strategy: editProvStrategy }
          : p
      ),
    }));
    addNotification({ title: t.notifProviderUpdated, message: t.notifProviderUpdatedMsg(sanitizedName.trim()), type: 'success' });
    setEditingProviderId(null);
  };

  const handleAddProvider = (e: React.FormEvent) => {
    e.preventDefault();
    const sanitizedName = sanitizeInput(provName);
    const sanitizedCode = sanitizeCode(provCode);
    const sanitizedDesc = sanitizeInput(provDescription);
    if (!sanitizedName.trim()) return;
    const newProvider: AuthProvider = {
      id: crypto.randomUUID(),
      name: sanitizedName.trim(),
      code: sanitizedCode.trim() || sanitizedName.trim().toUpperCase().replace(/\s+/g, '_').substring(0, 20),
      description: sanitizedDesc.trim(),
      strategy: provStrategy,
      isActive: true,
    };
    setProvidersData((prev) => ({ ...prev, [selectedId]: [...(prev[selectedId] || []), newProvider] }));
    setProvName(''); setProvCode(''); setProvDescription(''); setIsAddingProvider(false);
    addNotification({ title: t.notifProviderAdded, message: t.notifProviderAddedMsg(newProvider.name, newProvider.strategy), type: 'success' });
  };

  const handleToggleProvider = (providerId: string) => {
    setProvidersData((prev) => ({
      ...prev,
      [selectedId]: (prev[selectedId] || []).map((p) => p.id === providerId ? { ...p, isActive: !p.isActive } : p),
    }));
    addNotification({ title: t.notifProviderModified, message: t.notifProviderModifiedMsg, type: 'info' });
  };

  const handleRemoveProvider = (providerId: string) => {
    setProvidersData((prev) => ({ ...prev, [selectedId]: (prev[selectedId] || []).filter((p) => p.id !== providerId) }));
    addNotification({ title: t.notifProviderRemoved, message: t.notifProviderRemovedMsg, type: 'warning' });
  };

  // ── Branding handler ──────────────────────────────────────────────────────

  const handleUpdateBranding = (e: React.FormEvent) => {
    e.preventDefault();
    if (brandLogo && !isValidPublicUrl(brandLogo)) {
      addNotification({ title: t.notifBrandingFailed, message: t.notifBrandingFailedMsg, type: 'error' });
      return;
    }
    const updated: BrandingConfig = {
      headlineText: sanitizeInput(brandHeadline),
      secondaryText: sanitizeInput(brandSecondary),
      primaryButtonLabel: sanitizeInput(brandButtonLabel),
      footerText: sanitizeInput(brandFooter),
      primaryColor: brandColor,
      backgroundStyle: brandBackground,
      logo: brandLogo.trim(),
      logoFormat: brandLogoFormat,
      customDomain: sanitizeInput(brandCustomDomain),
      magicLinkFallbackEnabled: brandMagicLink,
      dnsVerificationStatus: brandingData[selectedId]?.dnsVerificationStatus ?? 'Pending',
    };
    setBrandingData((prev) => ({ ...prev, [selectedId]: updated }));
    addNotification({ title: t.notifBrandingApplied, message: t.notifBrandingMsg(brandColor), type: 'success' });
  };

  // ── New tenant form success ───────────────────────────────────────────────

  const handleCreateSuccess = (newTenantId: string) => {
    setPage(1);
    setAppliedQuery({ criteria: 'name', term: '' });
    setSearchValue('');
    applyTenantSelection(newTenantId);
  };

  // ── List controls ─────────────────────────────────────────────────────────

  const handleQuerySubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    if (searchCriteria === 'id' && searchValue.trim()) handleSelectTenant(searchValue.trim());
    setAppliedQuery({ criteria: searchCriteria, term: searchValue });
  };

  const handleResetQuery = () => {
    setSearchValue('');
    setAppliedQuery({ criteria: 'name', term: '' });
    setPage(1);
  };

  // ── Derived list data ─────────────────────────────────────────────────────

  let processedTenants = [...knownTenants];
  if (activeFilter !== 'all') processedTenants = processedTenants.filter((t) => t.status === activeFilter);
  if (appliedQuery.term.trim()) {
    const query = appliedQuery.term.toLowerCase().trim();
    processedTenants = processedTenants.filter((t) =>
      appliedQuery.criteria === 'name' ? t.name.toLowerCase().includes(query) :
      appliedQuery.criteria === 'code' ? t.code.toLowerCase().includes(query) :
      t.tenantId.toLowerCase().includes(query)
    );
  }
  processedTenants.sort((a, b) => {
    const va = (sortBy === 'name' ? a.name : sortBy === 'code' ? a.code : a.status) || '';
    const vb = (sortBy === 'name' ? b.name : sortBy === 'code' ? b.code : b.status) || '';
    return sortOrder === 'asc' ? va.localeCompare(vb) : vb.localeCompare(va);
  });

  const totalItems = processedTenants.length;
  const totalPages = Math.ceil(totalItems / pageSize);
  const startIndex = (page - 1) * pageSize;
  const paginatedTenants = processedTenants.slice(startIndex, startIndex + pageSize);

  // ── M3DataView option arrays ───────────────────────────────────────────────

  const criteriaOptions: QueryCriteriaOption[] = [
    { label: t.byName, value: 'name' },
    { label: t.byCode, value: 'code' },
    { label: t.byTenantId, value: 'id' },
  ];
  const filterOptions: FilterOption[] = [
    { label: t.allStatuses, value: 'all' },
    { label: t.active, value: 'Active' },
    { label: t.suspended, value: 'Suspended' },
  ];
  const sortOptions: SortOption[] = [
    { label: t.sortByName, value: 'name' },
    { label: t.sortByCode, value: 'code' },
    { label: t.sortByStatus, value: 'status' },
  ];

  // ── Render helpers ────────────────────────────────────────────────────────

  const statusBadge = (status: string) => {
    const label = status === 'Active' ? t.active : status === 'Suspended' ? t.suspended : t.pending;
    const cls = status === 'Active'
      ? 'bg-emerald-500/10 border-emerald-500/25 text-emerald-500'
      : status === 'Suspended'
      ? 'bg-rose-500/10 border-rose-500/25 text-rose-500'
      : 'bg-amber-500/10 border-amber-500/25 text-amber-500';
    return <span className={`text-[10px] font-medium px-2.5 py-0.5 rounded-full border ${cls}`}>{label}</span>;
  };

  const renderList = () => (
    <div className="overflow-x-auto border border-m3-outline/25 rounded-xl bg-m3-surface-container/20">
      <table className="w-full text-left border-collapse">
        <thead>
          <tr className="border-b border-m3-outline/20 text-xs font-medium text-m3-secondary bg-m3-surface-container/40">
            <th className="py-3.5 px-5">{t.colTenantName}</th>
            <th className="py-3.5 px-4">{t.colCode}</th>
            <th className="py-3.5 px-4">{t.colCategory}</th>
            <th className="py-3.5 px-4">{t.colStatus}</th>
            <th className="py-3.5 px-5 text-right">{t.colAction}</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-m3-outline/10 text-sm">
          {paginatedTenants.map((ten) => {
            const isSelected = selectedId === ten.tenantId;
            return (
              <tr
                key={ten.tenantId}
                onClick={() => handleSelectTenant(ten.tenantId)}
                className={`group cursor-pointer transition-colors duration-150 ${
                  isSelected
                    ? 'bg-m3-primary-container/30 text-m3-on-primary-container'
                    : 'hover:bg-m3-primary/5 text-m3-secondary hover:text-m3-on-surface'
                }`}
              >
                <td className="py-3.5 px-5">
                  <div className="flex items-center gap-3">
                    <div className={`p-2 rounded-lg border transition-colors ${
                      isSelected
                        ? 'bg-m3-primary text-white border-m3-primary'
                        : 'bg-m3-surface-container/60 border-m3-outline/20 text-m3-secondary group-hover:text-m3-primary group-hover:border-m3-primary/30'
                    }`}>
                      <Building2 className="w-4 h-4" />
                    </div>
                    <div>
                      <p className="font-medium text-m3-on-surface">{ten.name}</p>
                      <p className="text-xs text-m3-secondary/60 truncate max-w-[170px] md:max-w-xs">{ten.companyReference || ten.type}</p>
                    </div>
                  </div>
                </td>
                <td className="py-3.5 px-4 font-mono text-xs font-medium text-m3-on-surface">{ten.code}</td>
                <td className="py-3.5 px-4 text-xs">{ten.type}</td>
                <td className="py-3.5 px-4">{statusBadge(ten.status)}</td>
                <td className="py-3.5 px-5 text-right">
                  <div className="flex items-center justify-end gap-1.5">
                    {isSelected && (
                      <span className="h-5 w-5 bg-m3-primary text-m3-on-primary rounded-full flex items-center justify-center">
                        <Check className="w-3 h-3" />
                      </span>
                    )}
                    <span className="text-xs font-medium text-m3-primary opacity-0 group-hover:opacity-100 group-hover:translate-x-0.5 transition-all flex items-center gap-1">
                      {t.manage} <ArrowRight className="w-3.5 h-3.5" />
                    </span>
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );

  const renderThumbnail = () => (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      {paginatedTenants.map((ten) => {
        const isSelected = selectedId === ten.tenantId;
        return (
          <M3Card
            key={ten.tenantId}
            onClick={() => handleSelectTenant(ten.tenantId)}
            variant={isSelected ? 'elevated' : 'filled'}
            className={`p-5 cursor-pointer border transition-all duration-150 hover:-translate-y-0.5 hover:shadow-md ${
              isSelected ? 'border-m3-primary bg-m3-primary-container/15' : 'border-m3-outline/25 hover:border-m3-primary/30'
            }`}
          >
            <div className="flex justify-between items-start gap-4">
              <div className="flex gap-3">
                <div className={`p-2.5 rounded-lg border ${isSelected ? 'bg-m3-primary text-white border-m3-primary' : 'bg-m3-primary/10 text-m3-primary border-m3-primary/10'}`}>
                  <Building2 className="w-5 h-5" />
                </div>
                <div>
                  <h4 className="text-sm font-medium text-m3-on-surface line-clamp-1">{ten.name}</h4>
                  <p className="font-mono text-xs text-m3-secondary/70 mt-0.5">{ten.code}</p>
                </div>
              </div>
              {statusBadge(ten.status)}
            </div>
            <div className="mt-4 pt-3 border-t border-m3-outline/10 grid grid-cols-2 gap-2 text-xs">
              <div>
                <p className="text-m3-secondary font-medium">{t.colCategory}</p>
                <p className="font-medium text-m3-on-surface mt-0.5">{ten.type}</p>
              </div>
              <div>
                <p className="text-m3-secondary font-medium">{t.ref}</p>
                <p className="font-medium text-m3-on-surface truncate mt-0.5">{ten.companyReference || '—'}</p>
              </div>
            </div>
          </M3Card>
        );
      })}
    </div>
  );

  const footerTelemetry = (
    <div className="flex items-center gap-3">
      <div className="flex items-center gap-1.5">
        <span className="h-2 w-2 rounded-full bg-m3-primary animate-pulse" />
        <span className="text-xs font-medium text-m3-secondary/80">
          {t.showing} {processedTenants.length === 0 ? 0 : startIndex + 1}–{Math.min(startIndex + pageSize, totalItems)} {t.of} {totalItems} {t.tenants}
        </span>
      </div>
      {appliedQuery.term.trim() && (
        <button onClick={handleResetQuery} className="text-xs font-medium text-rose-500 hover:underline flex items-center gap-1">
          <Info className="w-3 h-3" /> {t.clearFilter}
        </button>
      )}
    </div>
  );

  // ─── JSX ─────────────────────────────────────────────────────────────────

  return (
    <div className="flex gap-4 flex-1 min-h-0 h-full">

      {/* ── Discard-changes dialog ── */}
      {showDiscardDialog && (
        <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 backdrop-blur-sm">
          <M3Card variant="elevated" className="p-6 max-w-sm w-full mx-4 border border-m3-outline/30 shadow-2xl space-y-4 animate-fadeIn">
            <div className="flex items-start gap-3">
              <div className="p-2 bg-amber-500/15 rounded-lg text-amber-500 flex-shrink-0">
                <ShieldAlert className="w-5 h-5" />
              </div>
              <div>
                <h3 className="text-sm font-semibold text-m3-on-surface">{t.unsavedChanges}</h3>
                <p className="text-xs text-m3-secondary mt-1 leading-relaxed">{t.unsavedChangesMsg}</p>
              </div>
            </div>
            <div className="flex gap-2.5 pt-1">
              <M3Button variant="outlined" onClick={() => setShowDiscardDialog(false)} className="flex-1">
                {t.cancelEdit}
              </M3Button>
              <M3Button variant="filled" onClick={confirmDiscard} className="flex-1 bg-m3-error hover:bg-m3-error/90 border-0">
                {t.discardChanges}
              </M3Button>
            </div>
          </M3Card>
        </div>
      )}

      {/* ── Left column — list ── */}
      <div className="flex flex-col min-w-0 flex-[6]">
        <M3DataView
          title={t.tenantMaintenance}
          subtitle={t.tenantMaintenanceSubtitle}
          searchPlaceholder={t.searchPlaceholder}
          searchCriteria={criteriaOptions}
          activeCriteria={searchCriteria}
          onCriteriaChange={setSearchCriteria}
          searchValue={searchValue}
          onSearchValueChange={setSearchValue}
          onSearchSubmit={handleQuerySubmit}
          onRegisterNew={() => setIsCreateOpen(true)}
          registerLabel={t.newBtn}
          viewMode={viewMode}
          onViewModeChange={setViewMode}
          sortOptions={sortOptions}
          sortBy={sortBy}
          onSortByChange={setSortBy}
          sortOrder={sortOrder}
          onSortOrderToggle={() => setSortOrder((o) => o === 'asc' ? 'desc' : 'asc')}
          filterOptions={filterOptions}
          activeFilter={activeFilter}
          onFilterChange={(val) => { setActiveFilter(val); setPage(1); }}
          isLoading={isLoadingList}
          isEmpty={processedTenants.length === 0}
          emptyLabel={t.noRecords}
          emptyTitle={t.dataViewEmptyTitle}
          loadingLabel={t.dataViewLoading}
          criteriaLabel={t.dataViewCriteriaLabel}
          searchTermLabel={t.dataViewSearchTermLabel}
          searchButtonLabel={t.dataViewSearchBtn}
          renderList={renderList}
          renderThumbnail={renderThumbnail}
          pagination={{ page, pageSize, totalItems, totalPages, onPageChange: setPage }}
          telemetryInfo={footerTelemetry}
        />
      </div>

      {/* ── Right column — detail ── */}
      <div className="flex flex-col min-w-0 flex-[4] overflow-y-auto space-y-4 pr-0.5">

        {isLoadingList ? (
          <M3Card variant="elevated" className="py-24 text-center text-sm text-m3-secondary border border-m3-outline/20">
            <RefreshCw className="w-8 h-8 animate-spin text-m3-primary mx-auto mb-3" />
            {t.loadingProfile}
          </M3Card>

        ) : activeTenant ? (
          <div key={selectedId} className="space-y-4">

            {/* ── Tenant profile banner ── */}
            <M3Card
              variant="elevated"
              className="p-5 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm group"
              onDoubleClick={() => !isTenantEditing && openTenantEdit()}
            >
              {!isTenantEditing ? (
                <>
                  <div className="flex justify-between items-start gap-4 pb-3.5 border-b border-m3-outline/15 mb-4">
                    <div className="flex gap-3 flex-1 min-w-0">
                      <div className="p-2 bg-m3-primary/10 rounded-lg text-m3-primary border border-m3-primary/10 self-start flex-shrink-0">
                        <Building className="w-5 h-5" />
                      </div>
                      <div className="min-w-0">
                        <h3 className="text-sm font-semibold text-m3-on-surface flex items-center gap-1.5 flex-wrap">
                          {activeTenant.name}
                          <span className="text-xs px-1.5 py-0.5 rounded font-medium bg-m3-outline/30 text-m3-secondary">
                            {activeTenant.code}
                          </span>
                        </h3>
                        {activeTenant.companyReference && (
                          <p className="text-xs text-m3-secondary mt-0.5">{activeTenant.companyReference}</p>
                        )}
                      </div>
                    </div>
                    <div className="flex items-center gap-2 flex-shrink-0">
                      {statusBadge(activeTenant.status)}
                      <IconButton tooltip={t.editTenant} onClick={openTenantEdit} className="opacity-0 group-hover:opacity-100">
                        <Pencil className="w-3.5 h-3.5" />
                      </IconButton>
                    </div>
                  </div>

                  <div className="flex items-center justify-between text-xs">
                    <div className="flex items-center gap-1.5 text-m3-secondary font-medium">
                      <Sliders className="w-3.5 h-3.5" />
                      <span>{t.stateControls}</span>
                    </div>
                    {activeTenant.status === 'Active' ? (
                      <M3Button
                        variant="outlined"
                        onClick={() => handleToggleStatus('Suspended')}
                        loading={isPendingMutation}
                        className="text-rose-500 border-rose-500/30 hover:bg-rose-500/10"
                      >
                        <ShieldAlert className="w-3.5 h-3.5 mr-1.5" /> {t.suspendBtn}
                      </M3Button>
                    ) : (
                      <M3Button
                        variant="filled"
                        onClick={() => handleToggleStatus('Active')}
                        loading={isPendingMutation}
                        className="bg-emerald-600 hover:bg-emerald-500"
                      >
                        <CheckCircle2 className="w-3.5 h-3.5 mr-1.5" /> {t.activateBtn}
                      </M3Button>
                    )}
                  </div>

                  <p className="text-xs text-m3-secondary/50 mt-3 text-center">{t.doubleClickToEdit}</p>
                </>
              ) : (
                /* ── Tenant inline-edit form ── */
                <div className="space-y-1 animate-fadeIn">
                  <div className="flex items-center justify-between mb-4">
                    <span className="text-sm font-medium text-m3-primary flex items-center gap-1.5">
                      <Pencil className="w-3.5 h-3.5" /> {t.editTenant}
                    </span>
                    <IconButton tooltip={t.cancelEdit} onClick={() => setIsTenantEditing(false)}>
                      <X className="w-3.5 h-3.5" />
                    </IconButton>
                  </div>

                  <M3TextField label={t.tenantName} required value={editName} onChange={(e) => setEditName(e.target.value)} />
                  <M3TextField label={t.tenantCode} required value={editCode} onChange={(e) => setEditCode(e.target.value.toUpperCase())} />
                  <M3TextField label={t.companyReference} value={editCompanyRef} onChange={(e) => setEditCompanyRef(e.target.value)} />

                  <M3Select label={t.tenantType} value={editType} onChange={(e) => setEditType(e.target.value)}>
                    {['INTERNAL', 'SUPPLIER', 'CLIENT'].map((tp) => <option key={tp} value={tp}>{tp}</option>)}
                  </M3Select>

                  <div className="flex gap-2 pt-1">
                    <M3Button variant="filled" onClick={saveTenantEdit} className="flex-1 flex items-center justify-center gap-1.5">
                      <Save className="w-3.5 h-3.5" /> {t.saveBtn}
                    </M3Button>
                    <M3Button variant="outlined" onClick={() => setIsTenantEditing(false)} className="flex-1">
                      {t.cancelEdit}
                    </M3Button>
                  </div>
                </div>
              )}
            </M3Card>

            {/* ── Tab bar ── */}
            <div className="flex border border-m3-outline/25 bg-m3-surface-container/20 rounded-xl p-1 select-none">
              {(['branches', 'providers', 'branding'] as const).map((tab) => {
                const icons = {
                  branches: <MapPin className="w-3.5 h-3.5" />,
                  providers: <Key className="w-3.5 h-3.5" />,
                  branding: <Palette className="w-3.5 h-3.5" />,
                };
                const labels = { branches: t.tabLocations, providers: t.tabAuthIdps, branding: t.tabBranding };
                return (
                  <button
                    key={tab}
                    onClick={() => setActiveConsoleTab(tab)}
                    className={`flex-1 flex items-center justify-center gap-1.5 py-2.5 rounded-lg text-xs font-medium transition-all duration-200 ${
                      activeConsoleTab === tab
                        ? 'bg-m3-primary text-m3-on-primary shadow-sm'
                        : 'text-m3-secondary hover:bg-m3-primary/10'
                    }`}
                  >
                    {icons[tab]} {labels[tab]}
                  </button>
                );
              })}
            </div>

            {/* ── Tab content ── */}
            <M3Card variant="elevated" className="p-5 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm">

              {/* ── Branches tab ── */}
              {activeConsoleTab === 'branches' && (
                <BranchManager tenantId={activeTenant.tenantId} />
              )}

              {/* ── Providers tab ── */}
              {activeConsoleTab === 'providers' && (
                <div className="space-y-4">
                  <div className="border-b border-m3-outline/10 pb-3">
                    <h3 className="text-sm font-semibold text-m3-on-surface">{t.identityProviders}</h3>
                    <p className="text-xs text-m3-secondary mt-0.5">{t.idpSubtitle}</p>
                  </div>

                  {!isAddingProvider && (
                    <M3Button
                      variant="tonal"
                      onClick={() => setIsAddingProvider(true)}
                      className="w-full flex items-center justify-center gap-1.5 border border-m3-primary/15"
                    >
                      <Plus className="w-4 h-4 text-m3-primary" /> {t.addProvider}
                    </M3Button>
                  )}

                  {isAddingProvider && (
                    <M3Card variant="outlined" className="p-4 bg-m3-surface-container/30 border-m3-primary/20 animate-fadeIn">
                      <div className="flex justify-between items-center border-b border-m3-outline/15 pb-2 mb-4">
                        <h4 className="text-sm font-medium text-m3-primary flex items-center gap-1.5">
                          <Plus className="w-3.5 h-3.5" /> {t.newProvider}
                        </h4>
                        <IconButton tooltip={t.cancelEdit} onClick={() => setIsAddingProvider(false)}>
                          <X className="w-3.5 h-3.5" />
                        </IconButton>
                      </div>
                      <form onSubmit={handleAddProvider} className="space-y-1">
                        <M3TextField label={t.providerName} required value={provName} onChange={(e) => setProvName(e.target.value)} placeholder="e.g. Okta SSO" />
                        <M3TextField label={t.providerCode} value={provCode} onChange={(e) => setProvCode(e.target.value.toUpperCase())} placeholder="e.g. OKTA_SSO" />
                        <M3Select label={t.protocolType} value={provStrategy} onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setProvStrategy(e.target.value as IdpStrategy)}>
                          <option value="OIDC">{t.strategyOIDC}</option>
                          <option value="SAML2">{t.strategySAML2}</option>
                          <option value="OAuth2">{t.strategyOAuth2}</option>
                        </M3Select>
                        <M3TextField label={t.providerDescription} value={provDescription} onChange={(e) => setProvDescription(e.target.value)} placeholder="e.g. https://login.microsoftonline.com/tenant-id" />
                        <M3Button variant="filled" type="submit" className="w-full mt-1">{t.saveProvider}</M3Button>
                      </form>
                    </M3Card>
                  )}

                  <div className="space-y-2.5">
                    {activeProviders.length === 0 ? (
                      <div className="py-8 text-center text-sm text-m3-secondary/75 flex flex-col items-center gap-2 border border-dashed border-m3-outline/25 rounded-xl p-4">
                        <Key className="w-5 h-5 text-m3-outline" />
                        {t.noIdps}
                      </div>
                    ) : (
                      activeProviders.map((p) =>
                        editingProviderId === p.id ? (
                          /* inline edit */
                          <div key={p.id} className="p-4 rounded-xl border border-m3-primary/30 bg-m3-surface-container/50 space-y-1 animate-fadeIn">
                            <div className="flex items-center justify-between mb-4">
                              <span className="text-sm font-medium text-m3-primary flex items-center gap-1.5">
                                <Pencil className="w-3.5 h-3.5" /> {t.editProvider}
                              </span>
                              <IconButton tooltip={t.cancelEdit} onClick={() => setEditingProviderId(null)}>
                                <X className="w-3.5 h-3.5" />
                              </IconButton>
                            </div>
                            <M3TextField label={t.providerName} required value={editProvName} onChange={(e) => setEditProvName(e.target.value)} />
                            <M3TextField label={t.providerCode} value={editProvCode} onChange={(e) => setEditProvCode(e.target.value.toUpperCase())} />
                            <M3Select label={t.protocolType} value={editProvStrategy} onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setEditProvStrategy(e.target.value as IdpStrategy)}>
                              <option value="OIDC">{t.strategyOIDC}</option>
                              <option value="SAML2">{t.strategySAML2}</option>
                              <option value="OAuth2">{t.strategyOAuth2}</option>
                            </M3Select>
                            <M3TextField label={t.providerDescription} value={editProvDescription} onChange={(e) => setEditProvDescription(e.target.value)} />
                            <div className="flex gap-2 pt-1">
                              <M3Button variant="filled" onClick={saveProviderEdit} className="flex-1 flex items-center justify-center gap-1.5">
                                <Save className="w-3.5 h-3.5" /> {t.saveBtn}
                              </M3Button>
                              <M3Button variant="outlined" onClick={() => setEditingProviderId(null)} className="flex-1">
                                {t.cancelEdit}
                              </M3Button>
                            </div>
                          </div>
                        ) : (
                          /* provider row */
                          <div
                            key={p.id}
                            className={`p-3.5 rounded-xl border flex items-center justify-between transition-colors bg-m3-surface-container/40 group/prov ${
                              p.isActive ? 'border-m3-outline/35' : 'border-m3-outline/15 opacity-60 bg-m3-outline/5'
                            }`}
                            onDoubleClick={() => openProviderEdit(p)}
                          >
                            <div className="space-y-1 min-w-0 flex-1">
                              <div className="flex items-center gap-1.5 flex-wrap">
                                <span className="font-medium text-sm text-m3-on-surface">{p.name}</span>
                                {p.code && (
                                  <span className="text-[10px] font-medium px-1.5 py-0.5 rounded bg-m3-outline/30 text-m3-secondary font-mono">{p.code}</span>
                                )}
                                <span className="text-[10px] font-medium px-1.5 py-0.5 rounded bg-m3-primary/10 text-m3-primary font-mono">{p.strategy}</span>
                              </div>
                              {p.description && (
                                <p className="text-xs font-mono text-m3-secondary truncate">{p.description}</p>
                              )}
                            </div>
                            <div className="flex items-center gap-1 flex-shrink-0 ml-2">
                              <IconButton tooltip={t.editProvider} onClick={() => openProviderEdit(p)} className="opacity-0 group-hover/prov:opacity-100">
                                <Pencil className="w-3.5 h-3.5" />
                              </IconButton>
                              <Tooltip content={p.isActive ? t.deactivate : t.reactivate}>
                                <button
                                  onClick={() => handleToggleProvider(p.id)}
                                  className={`p-1.5 rounded-full transition-all border ${
                                    p.isActive
                                      ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-500 hover:bg-emerald-500/20'
                                      : 'bg-rose-500/10 border-rose-500/20 text-rose-500 hover:bg-rose-500/20'
                                  }`}
                                >
                                  <Check className="w-3.5 h-3.5" />
                                </button>
                              </Tooltip>
                              <IconButton tooltip={t.removeLocation} onClick={() => handleRemoveProvider(p.id)} className="hover:text-m3-error hover:bg-m3-error/10">
                                <Trash2 className="w-3.5 h-3.5" />
                              </IconButton>
                            </div>
                          </div>
                        )
                      )
                    )}
                  </div>
                </div>
              )}

              {/* ── Branding tab ── */}
              {activeConsoleTab === 'branding' && (
                <div className="space-y-4">
                  <div className="flex justify-between items-center border-b border-m3-outline/10 pb-3">
                    <div>
                      <h3 className="text-sm font-semibold text-m3-on-surface">{t.customBranding}</h3>
                      <p className="text-xs text-m3-secondary mt-0.5">{t.brandingSubtitle}</p>
                    </div>
                    {/* DNS status badge */}
                    {(() => {
                      const dns = brandingData[selectedId]?.dnsVerificationStatus ?? 'Pending';
                      const cls =
                        dns === 'Verified' ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-500'
                        : dns === 'Failed'   ? 'bg-rose-500/10 border-rose-500/20 text-rose-500'
                        : 'bg-amber-500/10 border-amber-500/20 text-amber-500';
                      const label = dns === 'Verified' ? t.brandDnsVerified : dns === 'Failed' ? t.brandDnsFailed : t.brandDnsPending;
                      return (
                        <Tooltip content={t.brandDnsStatus}>
                          <span className={`text-[10px] font-medium px-2.5 py-1 rounded-full border cursor-default ${cls}`}>{label}</span>
                        </Tooltip>
                      );
                    })()}
                  </div>

                  <form onSubmit={handleUpdateBranding} className="space-y-1">

                    {/* Section: content */}
                    <p className="text-xs font-medium text-m3-secondary mb-3">{t.brandingContent}</p>

                    <M3TextField label={t.brandHeadline} value={brandHeadline} onChange={(e) => setBrandHeadline(e.target.value)} placeholder="e.g. Welcome to Ransa Portal" />
                    <M3TextField label={t.brandSecondary} value={brandSecondary} onChange={(e) => setBrandSecondary(e.target.value)} placeholder="e.g. Sign in to continue" />
                    <M3TextField label={t.brandButtonLabel} value={brandButtonLabel} onChange={(e) => setBrandButtonLabel(e.target.value)} placeholder="e.g. Sign in" />
                    <M3TextField label={t.brandFooter} value={brandFooter} onChange={(e) => setBrandFooter(e.target.value)} placeholder="e.g. © 2025 Ransa Perú S.A." />

                    {/* Section: visual */}
                    <p className="text-xs font-medium text-m3-secondary mb-3 mt-2">{t.brandingVisual}</p>

                    {/* Color picker — M3 outlined field style */}
                    <div className="relative mb-4">
                      <label className="absolute left-4 px-1 -mx-1 text-xs font-normal bg-m3-surface text-m3-secondary top-0 -translate-y-1/2 z-10 pointer-events-none">
                        {t.brandPrimaryColor}
                      </label>
                      <div className="w-full h-14 px-4 flex items-center gap-3 border-[1.5px] border-m3-outline rounded-[4px] bg-m3-surface-container/30 hover:border-m3-on-surface transition-colors">
                        <input
                          type="color"
                          value={brandColor}
                          onChange={(e) => setBrandColor(e.target.value)}
                          className="h-8 w-10 rounded border-0 bg-transparent cursor-pointer p-0 flex-shrink-0"
                        />
                        <span className="font-mono text-sm text-m3-on-surface">{brandColor}</span>
                      </div>
                    </div>

                    <M3Select label={t.brandBackground} value={brandBackground} onChange={(e) => setBrandBackground(e.target.value)}>
                      <option value="solid">{t.brandBgSolid}</option>
                      <option value="gradient-subtle">{t.brandBgGradientSubtle}</option>
                      <option value="gradient-bold">{t.brandBgGradientBold}</option>
                    </M3Select>

                    {/* Logo URL + format side-by-side */}
                    <div className="flex gap-3 items-start">
                      <div className="flex-1">
                        <M3TextField label={t.brandLogoUrl} value={brandLogo} onChange={(e) => setBrandLogo(e.target.value)} placeholder="https://logo.ransa.pe/logo.png" className="mb-0" />
                      </div>
                      <div className="w-28 flex-shrink-0">
                        <M3Select label={t.brandLogoFormat} value={brandLogoFormat} onChange={(e) => setBrandLogoFormat(e.target.value)} className="mb-0">
                          <option value="png">PNG</option>
                          <option value="svg">SVG</option>
                          <option value="jpeg">JPEG</option>
                        </M3Select>
                      </div>
                    </div>

                    {brandLogo && isValidPublicUrl(brandLogo) && (
                      <div className="mt-2 p-3 bg-m3-surface-container/60 rounded-lg border border-m3-outline/25 flex items-center justify-between gap-4">
                        <span className="text-xs font-medium text-m3-secondary">{t.brandLogoPreview}</span>
                        <img
                          src={brandLogo}
                          alt="Logo preview"
                          className="h-6 w-auto object-contain rounded border border-m3-outline/30 bg-white p-0.5"
                          onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }}
                        />
                      </div>
                    )}

                    {/* Section: domain & auth */}
                    <p className="text-xs font-medium text-m3-secondary mb-3 mt-6">{t.brandingDomain}</p>

                    <M3TextField label={t.brandCustomDomain} value={brandCustomDomain} onChange={(e) => setBrandCustomDomain(e.target.value)} placeholder="e.g. auth.ransa.pe" />

                    {/* Magic link toggle — M3 outlined field style */}
                    <div className="relative mb-4">
                      <label className="absolute left-4 px-1 -mx-1 text-xs font-normal bg-m3-surface text-m3-secondary top-0 -translate-y-1/2 z-10 pointer-events-none">
                        {t.brandMagicLink}
                      </label>
                      <div className="w-full h-14 px-4 flex items-center justify-between border-[1.5px] border-m3-outline rounded-[4px] bg-m3-surface-container/30 hover:border-m3-on-surface transition-colors">
                        <span className="text-sm text-m3-secondary">{brandMagicLink ? t.active : t.suspended}</span>
                        <button
                          type="button"
                          onClick={() => setBrandMagicLink(!brandMagicLink)}
                          className={`relative inline-flex h-6 w-11 flex-shrink-0 items-center rounded-full transition-colors focus:outline-none ${
                            brandMagicLink ? 'bg-m3-primary' : 'bg-m3-outline/50'
                          }`}
                        >
                          <span className={`inline-block h-4 w-4 rounded-full bg-white shadow transition-transform ${brandMagicLink ? 'translate-x-6' : 'translate-x-1'}`} />
                        </button>
                      </div>
                    </div>

                    <M3Button variant="filled" type="submit" className="w-full">
                      {t.applyBranding}
                    </M3Button>
                  </form>
                </div>
              )}

            </M3Card>
          </div>

        ) : (
          <M3Card variant="elevated" className="p-6 text-center text-sm text-m3-secondary border border-m3-outline/20 bg-m3-surface-container/10">
            {t.selectTenant}
          </M3Card>
        )}
      </div>

      <TenantForm isOpen={isCreateOpen} onClose={() => setIsCreateOpen(false)} onSuccess={handleCreateSuccess} />
    </div>
  );
};

export default TenantDashboardScreen;

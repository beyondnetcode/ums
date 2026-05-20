import React, { useState, useEffect } from 'react';
import { 
  useGetTenant, 
  useActivateTenant, 
  useSuspendTenant 
} from '../../../application/identity/hooks/use-tenant';
import { TenantForm } from '../components/TenantForm';
import { BranchManager } from '../components/BranchManager';
import { M3Card } from '../../shared/components/M3Card';
import { M3Button } from '../../shared/components/M3Button';
import { M3TextField } from '../../shared/components/M3TextField';
import { M3DataView, SortOption, FilterOption, QueryCriteriaOption } from '../../shared/components/M3DataView';
import { Tenant } from '../../../domain/identity/models/tenant.model';
import { useNotificationStore } from '../../../application/stores/notification.store';
import { 
  Building2, 
  ArrowRight,
  RefreshCw,
  Sliders,
  ShieldAlert,
  CheckCircle2,
  GitFork,
  Check,
  AlertTriangle,
  Building,
  Info,
  MapPin,
  Key,
  Palette,
  Plus,
  Trash2,
  X
} from 'lucide-react';

interface AuthProvider {
  id: string;
  name: string;
  type: 'OIDC' | 'SAML2' | 'OAuth2';
  clientId: string;
  isActive: boolean;
}

interface BrandingConfig {
  welcomeTitle: string;
  primaryColor: string;
  logoUrl: string;
}

export const TenantDashboardScreen: React.FC = () => {
  const addNotification = useNotificationStore((state) => state.addNotification);

  // Collection of baseline tenants (simulating core database index)
  const [knownTenants, setKnownTenants] = useState<Tenant[]>([
    { 
      tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6', 
      code: 'DEV_ROOT', 
      name: 'Default Developer Tenant (Root)', 
      type: 'Enterprise', 
      status: 'Active', 
      companyReference: 'REF_ROOT_001', 
      parentTenantId: null 
    },
    { 
      tenantId: 'c9b736b4-6a84-48f8-b34d-176bc5a6d542', 
      code: 'ACME_INT', 
      name: 'Acme Global Logistics', 
      type: 'Corporate', 
      status: 'Active', 
      companyReference: 'ACME-902', 
      parentTenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6' 
    },
    { 
      tenantId: 'a3f5b9d2-7c3d-4c8e-a9b0-123456789abc', 
      code: 'CONTOSO_EMEA', 
      name: 'Contoso Retail EMEA', 
      type: 'Subsidiary', 
      status: 'Suspended', 
      companyReference: 'CONT-EMEA-88', 
      parentTenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6' 
    },
    { 
      tenantId: '9e8d7c6b-5a4f-3e2d-1c0b-9876543210fe', 
      code: 'NWD_IND', 
      name: 'Northwind Industrial', 
      type: 'Industrial', 
      status: 'Active', 
      companyReference: 'NWD-CORE-10', 
      parentTenantId: null 
    },
    { 
      tenantId: '5f4e3d2c-1b0a-9f8e-7d6c-543210987654', 
      code: 'VNG_HLTH', 
      name: 'Vanguard Health Systems', 
      type: 'Medical', 
      status: 'Active', 
      companyReference: 'VNG-HEALTH-5', 
      parentTenantId: null 
    },
    { 
      tenantId: 'f3e2d1c0-b9a8-7f6e-5d4c-321098765432', 
      code: 'APX_FIN', 
      name: 'Apex Financial Group', 
      type: 'Financial', 
      status: 'Pending', 
      companyReference: 'APX-GLOBAL-7', 
      parentTenantId: null 
    }
  ]);

  // Selected details state
  const [selectedId, setSelectedId] = useState<string>('3fa85f64-5717-4562-b3fc-2c963f66afa6');
  
  // Custom M3 Child structures stored per tenant
  const [providersData, setProvidersData] = useState<{ [tenantId: string]: AuthProvider[] }>({
    '3fa85f64-5717-4562-b3fc-2c963f66afa6': [
      { id: '1', name: 'Google Workspace Identity', type: 'OIDC', clientId: '902318-abc.apps.googleusercontent.com', isActive: true },
      { id: '2', name: 'Azure Active Directory', type: 'SAML2', clientId: 'tenant-azure-992-ad', isActive: false }
    ],
    'c9b736b4-6a84-48f8-b34d-176bc5a6d542': [
      { id: '3', name: 'Okta Enterprise SSO', type: 'OIDC', clientId: 'acme-sso-okta-882', isActive: true }
    ]
  });

  const [brandingData, setBrandingData] = useState<{ [tenantId: string]: BrandingConfig }>({
    '3fa85f64-5717-4562-b3fc-2c963f66afa6': {
      welcomeTitle: 'Welcome to Developer Workspace',
      primaryColor: 'HSL(262, 52%, 47%)',
      logoUrl: 'https://images.unsplash.com/photo-1560179707-f14e90ef3623?w=80&auto=format&fit=crop&q=60'
    },
    'c9b736b4-6a84-48f8-b34d-176bc5a6d542': {
      welcomeTitle: 'Acme Global Logistics Console',
      primaryColor: 'HSL(200, 70%, 40%)',
      logoUrl: 'https://images.unsplash.com/photo-1516880711640-ef7db81be3e1?w=80&auto=format&fit=crop&q=60'
    }
  });

  // M3 Console active tab: 'branches' | 'providers' | 'branding'
  const [activeConsoleTab, setActiveConsoleTab] = useState<'branches' | 'providers' | 'branding'>('branches');

  // M3DataView State Control
  const [viewMode, setViewMode] = useState<'list' | 'thumbnail'>('list');
  const [searchCriteria, setSearchCriteria] = useState<string>('name');
  const [searchValue, setSearchValue] = useState<string>('');
  const [appliedQuery, setAppliedQuery] = useState<{ criteria: string; term: string }>({ criteria: 'name', term: '' });
  const [activeFilter, setActiveFilter] = useState<string>('all');
  const [sortBy, setSortBy] = useState<string>('name');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  const [page, setPage] = useState<number>(1);
  const pageSize = 5;

  const [isCreateOpen, setIsCreateOpen] = useState(false);

  // Auth Provider dynamic adding form states
  const [isAddingProvider, setIsAddingProvider] = useState(false);
  const [provName, setProvName] = useState('');
  const [provType, setProvType] = useState<'OIDC' | 'SAML2' | 'OAuth2'>('OIDC');
  const [provClientId, setProvClientId] = useState('');

  // Branding dynamic configuration local form states
  const [brandTitle, setBrandTitle] = useState('');
  const [brandColor, setBrandColor] = useState('');
  const [brandLogo, setBrandLogo] = useState('');

  // Active query hook for the selected tenant or custom queried ID
  const { data: tenant, isLoading, error } = useGetTenant(selectedId || null);

  // Graceful local fallback if backend does not yet contain seed records
  const activeTenant = tenant || knownTenants.find((t) => t.tenantId === selectedId);

  // Mutation hooks
  const activateMutation = useActivateTenant(selectedId);
  const suspendMutation = useSuspendTenant(selectedId);

  // Synchronized status toggling with instant local state fallback
  const handleToggleStatus = (newStatus: 'Active' | 'Suspended') => {
    // 1. Update local list state immediately for instant rendering
    setKnownTenants((prev) =>
      prev.map((t) => (t.tenantId === selectedId ? { ...t, status: newStatus } : t))
    );
    
    // 2. Add system notification toast
    addNotification({
      title: newStatus === 'Active' ? 'Tenant Activated' : 'Tenant Suspended',
      message: `The selected tenant status was successfully set to ${newStatus}.`,
      type: newStatus === 'Active' ? 'success' : 'warning',
    });

    // 3. Sync with backend API silently (graceful fallback if 404/failure)
    if (newStatus === 'Active') {
      activateMutation.mutate(undefined, {
        onError: (err) => {
          console.warn("API Activation bypassed. Handled locally:", err.message);
        }
      });
    } else {
      suspendMutation.mutate(undefined, {
        onError: (err) => {
          console.warn("API Suspension bypassed. Handled locally:", err.message);
        }
      });
    }
  };

  // Synchronize dynamic updates from the GET detail endpoint back into the local list state
  useEffect(() => {
    if (tenant) {
      setKnownTenants((prev) => {
        const exists = prev.some((t) => t.tenantId === tenant.tenantId);
        if (!exists) {
          return [tenant, ...prev];
        }
        return prev.map((t) => t.tenantId === tenant.tenantId ? { ...t, status: tenant.status } : t);
      });
    }
  }, [tenant]);

  // Synchronize branding values when changing the active tenant selection
  useEffect(() => {
    if (selectedId) {
      const activeBrand = brandingData[selectedId] || {
        welcomeTitle: 'UMS Corporate Portal',
        primaryColor: 'HSL(262, 52%, 47%)',
        logoUrl: ''
      };
      setBrandTitle(activeBrand.welcomeTitle);
      setBrandColor(activeBrand.primaryColor);
      setBrandLogo(activeBrand.logoUrl);
      
      // Reset creation panels
      setIsAddingProvider(false);
      setProvName('');
      setProvClientId('');
    }
  }, [selectedId, brandingData]);

  // Handle Tenant Creation Success
  const handleCreateSuccess = (newTenantId: string, newCode: string, newName: string) => {
    const newTenant: Tenant = {
      tenantId: newTenantId,
      code: newCode,
      name: newName,
      type: 'Enterprise',
      status: 'Active',
      companyReference: 'NEW-REF-' + newCode,
      parentTenantId: null
    };

    setKnownTenants((prev) => [newTenant, ...prev]);
    setSelectedId(newTenantId);
    setPage(1);
    setAppliedQuery({ criteria: 'name', term: '' });
    setSearchValue('');
    setActiveConsoleTab('branches');
  };

  // Query database submit handler (for loading specific GUID or local filters)
  const handleQuerySubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    
    // If querying specifically by GUID, try to retrieve it directly from the endpoint
    if (searchCriteria === 'id' && searchValue.trim()) {
      const targetId = searchValue.trim();
      setSelectedId(targetId);
    }
    
    setAppliedQuery({ criteria: searchCriteria, term: searchValue });
  };

  // Reset Query Criteria Filter helper
  const handleResetQuery = () => {
    setSearchValue('');
    setAppliedQuery({ criteria: 'name', term: '' });
    setPage(1);
  };

  // Interactive local filter/sort/pagination logic
  let processedTenants = [...knownTenants];

  // 1. Status Filter
  if (activeFilter !== 'all') {
    processedTenants = processedTenants.filter(t => t.status === activeFilter);
  }

  // 2. Query Criteria Search Filter
  if (appliedQuery.term.trim()) {
    const query = appliedQuery.term.toLowerCase().trim();
    if (appliedQuery.criteria === 'name') {
      processedTenants = processedTenants.filter(t => t.name.toLowerCase().includes(query));
    } else if (appliedQuery.criteria === 'code') {
      processedTenants = processedTenants.filter(t => t.code.toLowerCase().includes(query));
    } else if (appliedQuery.criteria === 'id') {
      processedTenants = processedTenants.filter(t => t.tenantId.toLowerCase().includes(query));
    }
  }

  // 3. Sorting
  processedTenants.sort((a, b) => {
    let valA = (sortBy === 'name' ? a.name : sortBy === 'code' ? a.code : a.status) || '';
    let valB = (sortBy === 'name' ? b.name : sortBy === 'code' ? b.code : b.status) || '';
    
    if (sortOrder === 'asc') {
      return valA.localeCompare(valB);
    } else {
      return valB.localeCompare(valA);
    }
  });

  // 4. Pagination slicing
  const totalItems = processedTenants.length;
  const totalPages = Math.ceil(totalItems / pageSize);
  const startIndex = (page - 1) * pageSize;
  const paginatedTenants = processedTenants.slice(startIndex, startIndex + pageSize);

  // Options configuration for M3DataView
  const criteriaOptions: QueryCriteriaOption[] = [
    { label: 'By Name', value: 'name' },
    { label: 'By Code', value: 'code' },
    { label: 'By Tenant ID (GUID)', value: 'id' }
  ];

  const filterOptions: FilterOption[] = [
    { label: 'All Statuses', value: 'all' },
    { label: 'Active', value: 'Active' },
    { label: 'Suspended', value: 'Suspended' }
  ];

  const sortOptions: SortOption[] = [
    { label: 'Sort: Name', value: 'name' },
    { label: 'Sort: Code', value: 'code' },
    { label: 'Sort: Status', value: 'status' }
  ];

  const isPendingMutation = activateMutation.isPending || suspendMutation.isPending;

  // Google/Okta identity provider handlers
  const activeProviders = providersData[selectedId] || [];

  const handleAddProvider = (e: React.FormEvent) => {
    e.preventDefault();
    if (!provName.trim() || !provClientId.trim()) return;

    const newProvider: AuthProvider = {
      id: Math.random().toString(36).substring(2, 9),
      name: provName.trim(),
      type: provType,
      clientId: provClientId.trim(),
      isActive: true
    };

    setProvidersData((prev) => ({
      ...prev,
      [selectedId]: [...(prev[selectedId] || []), newProvider]
    }));

    setProvName('');
    setProvClientId('');
    setIsAddingProvider(false);

    addNotification({
      title: 'Auth Provider Added',
      message: `Successfully linked identity scheme: '${newProvider.name}' [${newProvider.type}].`,
      type: 'success'
    });
  };

  const handleToggleProvider = (providerId: string) => {
    setProvidersData((prev) => ({
      ...prev,
      [selectedId]: (prev[selectedId] || []).map((p) => 
        p.id === providerId ? { ...p, isActive: !p.isActive } : p
      )
    }));

    addNotification({
      title: 'Provider State Modified',
      message: 'Client authentication protocol status successfully updated.',
      type: 'info'
    });
  };

  const handleRemoveProvider = (providerId: string) => {
    setProvidersData((prev) => ({
      ...prev,
      [selectedId]: (prev[selectedId] || []).filter((p) => p.id !== providerId)
    }));

    addNotification({
      title: 'Auth Provider Disconnected',
      message: 'Third-party identity configurations dismantled successfully.',
      type: 'warning'
    });
  };

  // Branding configurations update handler
  const handleUpdateBranding = (e: React.FormEvent) => {
    e.preventDefault();
    
    setBrandingData((prev) => ({
      ...prev,
      [selectedId]: {
        welcomeTitle: brandTitle.trim(),
        primaryColor: brandColor.trim(),
        logoUrl: brandLogo.trim()
      }
    }));

    addNotification({
      title: 'Branding Customized',
      message: `Theme profiles updated. Primary style token: ${brandColor}`,
      type: 'success'
    });
  };

  // Custom List/Table View Renderer
  const renderList = () => (
    <div className="overflow-x-auto border border-m3-outline/25 rounded-2xl bg-m3-surface-container/20">
      <table className="w-full text-left border-collapse">
        <thead>
          <tr className="border-b border-m3-outline/20 text-[10px] font-black uppercase tracking-wider text-m3-secondary bg-m3-surface-container/40">
            <th className="py-4.5 px-5">Tenant Name</th>
            <th className="py-4.5 px-4">Code</th>
            <th className="py-4.5 px-4">Category</th>
            <th className="py-4.5 px-4">Status</th>
            <th className="py-4.5 px-5 text-right">Action</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-m3-outline/10 text-xs font-semibold">
          {paginatedTenants.map((t) => {
            const isSelected = selectedId === t.tenantId;
            return (
              <tr 
                key={t.tenantId}
                onClick={() => setSelectedId(t.tenantId)}
                className={`group cursor-pointer transition-colors duration-150 ${
                  isSelected 
                    ? 'bg-m3-primary-container/30 text-m3-on-primary-container' 
                    : 'hover:bg-m3-primary/5 text-m3-secondary hover:text-m3-on-surface'
                }`}
              >
                <td className="py-4 px-5">
                  <div className="flex items-center gap-3">
                    <div className={`p-2 rounded-xl border transition-colors ${
                      isSelected 
                        ? 'bg-m3-primary text-white border-m3-primary' 
                        : 'bg-m3-surface-container/60 border-m3-outline/20 text-m3-secondary group-hover:text-m3-primary group-hover:border-m3-primary/30'
                    }`}>
                      <Building2 className="w-4 h-4" />
                    </div>
                    <div>
                      <p className="font-extrabold text-m3-on-surface">{t.name}</p>
                      <p className="font-mono text-[9px] opacity-60 truncate max-w-[170px] md:max-w-xs">{t.tenantId}</p>
                    </div>
                  </div>
                </td>
                <td className="py-4 px-4 font-mono font-bold text-[10px] text-m3-on-surface">{t.code}</td>
                <td className="py-4 px-4">{t.type}</td>
                <td className="py-4 px-4">
                  <span className={`text-[8px] font-black uppercase tracking-wider px-2.5 py-0.5 rounded-full border ${
                    t.status === 'Active'
                      ? 'bg-emerald-500/10 border-emerald-500/25 text-emerald-500'
                      : t.status === 'Suspended'
                      ? 'bg-rose-500/10 border-rose-500/25 text-rose-500'
                      : 'bg-amber-500/10 border-amber-500/25 text-amber-500'
                  }`}>
                    {t.status}
                  </span>
                </td>
                <td className="py-4 px-5 text-right">
                  <div className="flex items-center justify-end gap-1.5">
                    {isSelected && (
                      <span className="h-5 w-5 bg-m3-primary text-m3-on-primary rounded-full flex items-center justify-center scale-90">
                        <Check className="w-3.5 h-3.5" />
                      </span>
                    )}
                    <span className="text-[10px] uppercase font-bold tracking-wider text-m3-primary opacity-0 group-hover:opacity-100 group-hover:translate-x-1 transition-all flex items-center gap-1">
                      Manage <ArrowRight className="w-3.5 h-3.5" />
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

  // Custom Thumbnail/Card Grid View Renderer
  const renderThumbnail = () => (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      {paginatedTenants.map((t) => {
        const isSelected = selectedId === t.tenantId;
        return (
          <M3Card 
            key={t.tenantId}
            onClick={() => setSelectedId(t.tenantId)}
            variant={isSelected ? "elevated" : "filled"}
            className={`p-5 cursor-pointer rounded-2xl border transition-all duration-150 hover:-translate-y-0.5 hover:shadow-md ${
              isSelected 
                ? 'border-m3-primary bg-m3-primary-container/15' 
                : 'border-m3-outline/25 hover:border-m3-primary/30'
            }`}
          >
            <div className="flex justify-between items-start gap-4">
              <div className="flex gap-3">
                <div className={`p-2.5 rounded-xl border ${
                  isSelected ? 'bg-m3-primary text-white border-m3-primary' : 'bg-m3-primary/10 text-m3-primary border-m3-primary/10'
                }`}>
                  <Building2 className="w-5 h-5" />
                </div>
                <div>
                  <h4 className="text-xs font-black text-m3-on-surface line-clamp-1">{t.name}</h4>
                  <p className="font-mono text-[9px] opacity-70 mt-0.5">{t.code}</p>
                </div>
              </div>
              <span className={`text-[8px] font-black uppercase tracking-wider px-2.5 py-0.5 rounded-full border ${
                t.status === 'Active'
                  ? 'bg-emerald-500/10 border-emerald-500/25 text-emerald-500'
                  : t.status === 'Suspended'
                  ? 'bg-rose-500/10 border-rose-500/25 text-rose-500'
                  : 'bg-amber-500/10 border-amber-500/25 text-amber-500'
              }`}>
                {t.status}
              </span>
            </div>

            <div className="mt-4 pt-3.5 border-t border-m3-outline/10 grid grid-cols-2 gap-2 text-[10px]">
              <div>
                <p className="text-m3-secondary uppercase font-bold tracking-wider opacity-75">Category</p>
                <p className="font-bold text-m3-on-surface">{t.type}</p>
              </div>
              <div>
                <p className="text-m3-secondary uppercase font-bold tracking-wider opacity-75">Ref</p>
                <p className="font-bold text-m3-on-surface truncate">{t.companyReference || 'None'}</p>
              </div>
            </div>
          </M3Card>
        );
      })}
    </div>
  );

  // Footer telemetry stats
  const footerTelemetry = (
    <div className="flex items-center gap-3">
      <div className="flex items-center gap-1.5">
        <span className="h-2 w-2 rounded-full bg-m3-primary animate-pulse" />
        <span className="text-[10px] uppercase tracking-wider text-m3-secondary/80">
          Showing {processedTenants.length === 0 ? 0 : startIndex + 1} - {Math.min(startIndex + pageSize, totalItems)} of {totalItems} Tenants
        </span>
      </div>
      {appliedQuery.term.trim() && (
        <button 
          onClick={handleResetQuery}
          className="text-[9px] uppercase tracking-widest text-rose-500 hover:underline font-extrabold flex items-center gap-1"
        >
          <Info className="w-3 h-3" /> Clear Criteria Filter
        </button>
      )}
    </div>
  );

  return (
    <div className="space-y-6">
      {/* Split Panel Layout */}
      <div className="grid grid-cols-1 xl:grid-cols-12 gap-6 items-start">
        
        {/* LEFT COLUMN: Data List View (7 columns) */}
        <div className="xl:col-span-7 space-y-6">
          <M3DataView
            title="Tenant Maintenance"
            subtitle="Explore registered corporate environments, query tenant blocks, and manage branch routing models."
            searchPlaceholder="Enter search parameter..."
            searchCriteria={criteriaOptions}
            activeCriteria={searchCriteria}
            onCriteriaChange={(val) => setSearchCriteria(val)}
            searchValue={searchValue}
            onSearchValueChange={(val) => setSearchValue(val)}
            onSearchSubmit={handleQuerySubmit}
            onRegisterNew={() => setIsCreateOpen(true)}
            registerLabel="New"
            viewMode={viewMode}
            onViewModeChange={(mode) => setViewMode(mode)}
            sortOptions={sortOptions}
            sortBy={sortBy}
            onSortByChange={(val) => setSortBy(val)}
            sortOrder={sortOrder}
            onSortOrderToggle={() => setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')}
            filterOptions={filterOptions}
            activeFilter={activeFilter}
            onFilterChange={(val) => {
              setActiveFilter(val);
              setPage(1);
            }}
            isLoading={isLoading}
            isEmpty={processedTenants.length === 0}
            emptyLabel="No matching records were located in the workspace index. Clear filters or query another GUID."
            renderList={renderList}
            renderThumbnail={renderThumbnail}
            pagination={{
              page,
              pageSize,
              totalItems,
              totalPages,
              onPageChange: (p) => setPage(p)
            }}
            telemetryInfo={footerTelemetry}
          />
        </div>

        {/* RIGHT COLUMN: Tabbed Configuration Console (5 columns) */}
        <div className="xl:col-span-5 space-y-5">
          {isLoading ? (
            <M3Card variant="elevated" className="py-24 text-center text-xs text-m3-secondary border border-m3-outline/20">
              <RefreshCw className="w-8 h-8 animate-spin text-m3-primary mx-auto mb-3" />
              Loading profile mapping...
            </M3Card>
          ) : activeTenant ? (
            <div className="space-y-4 animate-fadeIn">
              
              {/* Tenant Profile Banner Card */}
              <M3Card variant="elevated" className="p-5 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm">
                <div className="flex justify-between items-start gap-4 pb-3.5 border-b border-m3-outline/15 mb-4">
                  <div className="flex gap-2">
                    <div className="p-2 bg-m3-primary/10 rounded-xl text-m3-primary border border-m3-primary/10 self-start">
                      <Building className="w-5 h-5" />
                    </div>
                    <div>
                      <h3 className="text-xs font-black text-m3-on-surface flex items-center gap-1.5 flex-wrap">
                        {activeTenant.name}
                        <span className="text-[8px] px-1.5 py-0.5 rounded font-black bg-m3-outline/30 text-m3-secondary uppercase">
                          {activeTenant.code}
                        </span>
                      </h3>
                      <p className="text-[9px] text-m3-secondary font-mono mt-0.5 max-w-[190px] truncate" title={activeTenant.tenantId}>
                        ID: {activeTenant.tenantId}
                      </p>
                    </div>
                  </div>

                  <span className={`text-[8px] font-black uppercase tracking-wider px-2 py-0.5 rounded-full border ${
                    activeTenant.status === 'Active'
                      ? 'bg-emerald-500/10 border-emerald-500/25 text-emerald-500'
                      : activeTenant.status === 'Suspended'
                      ? 'bg-rose-500/10 border-rose-500/25 text-rose-500'
                      : 'bg-amber-500/10 border-amber-500/25 text-amber-500'
                  }`}>
                    {activeTenant.status}
                  </span>
                </div>

                {/* Operations quick suspension switches */}
                <div className="flex items-center justify-between text-[11px] font-bold">
                  <div className="flex items-center gap-1 text-m3-secondary">
                    <Sliders className="w-3.5 h-3.5" />
                    <span>State controls</span>
                  </div>
                  {activeTenant.status === 'Active' ? (
                    <M3Button
                      variant="outlined"
                      onClick={() => handleToggleStatus('Suspended')}
                      loading={isPendingMutation}
                      className="text-rose-500 border-rose-500/25 hover:bg-rose-500/10 text-[9px] px-3.5 py-1.5 h-8.5 font-black uppercase tracking-wider"
                    >
                      <ShieldAlert className="w-3 h-3 mr-1" /> Suspend
                    </M3Button>
                  ) : (
                    <M3Button
                      variant="filled"
                      onClick={() => handleToggleStatus('Active')}
                      loading={isPendingMutation}
                      className="bg-emerald-600 hover:bg-emerald-500 text-[9px] px-3.5 py-1.5 h-8.5 font-black uppercase tracking-wider"
                    >
                      <CheckCircle2 className="w-3 h-3 mr-1" /> Activate
                    </M3Button>
                  )}
                </div>
              </M3Card>

              {/* M3 Tab Bar Navigation */}
              <div className="flex border border-m3-outline/25 bg-m3-surface-container/20 rounded-2xl p-1 select-none">
                <button
                  onClick={() => setActiveConsoleTab('branches')}
                  className={`flex-1 flex items-center justify-center gap-1.5 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-wider transition-all duration-200 ${
                    activeConsoleTab === 'branches'
                      ? 'bg-m3-primary text-m3-on-primary shadow-sm font-extrabold'
                      : 'text-m3-secondary hover:bg-m3-primary/10'
                  }`}
                >
                  <MapPin className="w-3.5 h-3.5" /> Locations
                </button>
                <button
                  onClick={() => setActiveConsoleTab('providers')}
                  className={`flex-1 flex items-center justify-center gap-1.5 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-wider transition-all duration-200 ${
                    activeConsoleTab === 'providers'
                      ? 'bg-m3-primary text-m3-on-primary shadow-sm font-extrabold'
                      : 'text-m3-secondary hover:bg-m3-primary/10'
                  }`}
                >
                  <Key className="w-3.5 h-3.5" /> Auth IDPs
                </button>
                <button
                  onClick={() => setActiveConsoleTab('branding')}
                  className={`flex-1 flex items-center justify-center gap-1.5 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-wider transition-all duration-200 ${
                    activeConsoleTab === 'branding'
                      ? 'bg-m3-primary text-m3-on-primary shadow-sm font-extrabold'
                      : 'text-m3-secondary hover:bg-m3-primary/10'
                  }`}
                >
                  <Palette className="w-3.5 h-3.5" /> Branding
                </button>
              </div>

              {/* Active Tab Panel content slot */}
              <M3Card variant="elevated" className="p-5 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm min-h-[300px]">
                
                {/* 1. BRANCHES TAB (Child Structures) */}
                {activeConsoleTab === 'branches' && (
                  <BranchManager tenantId={activeTenant.tenantId} />
                )}

                {/* 2. AUTH PROVIDERS TAB (Child Structures) */}
                {activeConsoleTab === 'providers' && (
                  <div className="space-y-4">
                    <div className="flex justify-between items-center border-b border-m3-outline/10 pb-3">
                      <div>
                        <h3 className="text-xs font-black uppercase tracking-wider text-m3-on-surface">
                          Identity Providers
                        </h3>
                        <p className="text-[9px] text-m3-secondary font-bold">
                          Configure federated OIDC or SAML systems.
                        </p>
                      </div>
                    </div>

                    {/* Prominent collapsible inline form */}
                    {!isAddingProvider ? (
                      <M3Button
                        variant="tonal"
                        onClick={() => setIsAddingProvider(true)}
                        className="w-full flex items-center justify-center gap-1.5 py-2 text-[9px] font-black uppercase tracking-wider border border-m3-primary/15"
                      >
                        <Plus className="w-3.5 h-3.5 text-m3-primary" /> Add Provider
                      </M3Button>
                    ) : (
                      <M3Card variant="outlined" className="p-4 rounded-2xl bg-m3-surface-container/30 border-m3-primary/20 animate-fadeIn">
                        <div className="flex justify-between items-center border-b border-m3-outline/15 pb-2 mb-3">
                          <h4 className="text-[9px] font-black uppercase tracking-wider text-m3-primary flex items-center gap-1">
                            <Plus className="w-3.5 h-3.5" /> New Identity Provider
                          </h4>
                          <button 
                            onClick={() => setIsAddingProvider(false)} 
                            className="p-1 rounded-full hover:bg-m3-primary/10 text-m3-secondary"
                          >
                            <X className="w-3.5 h-3.5" />
                          </button>
                        </div>

                        <form onSubmit={handleAddProvider} className="space-y-3">
                          <M3TextField
                            label="Provider Name"
                            required
                            value={provName}
                            onChange={(e) => setProvName(e.target.value)}
                            placeholder="e.g. Okta SSO"
                          />

                          <div className="flex flex-col w-full relative mb-4">
                            <label className="block text-[11px] font-bold text-m3-primary dark:text-m3-primary/80 uppercase tracking-wider mb-2 ml-1">
                              Protocol Type
                            </label>
                            <select
                              value={provType}
                              onChange={(e: any) => setProvType(e.target.value)}
                              className="w-full px-4 py-3.5 text-sm rounded-2xl border border-m3-outline bg-m3-surface-container/40 dark:bg-m3-surface-container/20 text-m3-on-surface focus:outline-none focus:border-m3-primary focus:ring-2 focus:ring-m3-primary/20 transition-all duration-300 cursor-pointer"
                            >
                              <option value="OIDC">OIDC (OpenID Connect)</option>
                              <option value="SAML2">SAML 2.0 Enterprise</option>
                              <option value="OAuth2">OAuth2 Generic</option>
                            </select>
                          </div>

                          <M3TextField
                            label="Client ID / Entity URL"
                            required
                            value={provClientId}
                            onChange={(e) => setProvClientId(e.target.value)}
                            placeholder="e.g. https://auth.acme.com"
                          />

                          <M3Button
                            variant="filled"
                            type="submit"
                            className="w-full text-[9px] py-1.5 h-8.5 font-black uppercase tracking-wider"
                          >
                            Save Provider
                          </M3Button>
                        </form>
                      </M3Card>
                    )}

                    {/* Providers listing */}
                    <div className="space-y-2.5 max-h-[260px] overflow-y-auto pr-1">
                      {activeProviders.length === 0 ? (
                        <div className="py-8 text-center text-[10px] text-m3-secondary/75 flex flex-col items-center justify-center gap-1 border border-dashed border-m3-outline/25 rounded-2xl p-4">
                          <Key className="w-5 h-5 text-m3-outline" />
                          No third-party IDPs connected yet.
                        </div>
                      ) : (
                        activeProviders.map((p) => (
                          <div 
                            key={p.id}
                            className={`p-3 rounded-xl border flex items-center justify-between transition-colors bg-m3-surface-container/40 ${
                              p.isActive ? 'border-m3-outline/35' : 'border-m3-outline/15 opacity-65 bg-m3-outline/5'
                            }`}
                          >
                            <div className="space-y-0.5 max-w-[80%]">
                              <div className="flex items-center gap-1.5 flex-wrap">
                                <span className="font-extrabold text-[11px] text-m3-on-surface">{p.name}</span>
                                <span className="text-[8px] font-black uppercase tracking-widest px-1.5 py-0.5 rounded bg-m3-primary/10 text-m3-primary font-mono">{p.type}</span>
                              </div>
                              <p className="text-[9px] font-mono text-m3-secondary truncate" title={p.clientId}>ID: {p.clientId}</p>
                            </div>

                            <div className="flex items-center gap-1 flex-shrink-0">
                              <button
                                onClick={() => handleToggleProvider(p.id)}
                                className={`p-1.5 rounded-full transition-all border ${
                                  p.isActive 
                                    ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-500 hover:bg-emerald-500/20' 
                                    : 'bg-rose-500/10 border-rose-500/20 text-rose-500 hover:bg-rose-500/20'
                                }`}
                                title={p.isActive ? 'Deactivate Provider' : 'Activate Provider'}
                              >
                                <Check className="w-3.5 h-3.5" />
                              </button>
                              <button
                                onClick={() => handleRemoveProvider(p.id)}
                                className="p-1.5 rounded-full hover:bg-m3-error/15 text-m3-secondary hover:text-m3-error transition-all"
                                title="Disconnect Provider"
                              >
                                <Trash2 className="w-3.5 h-3.5" />
                              </button>
                            </div>
                          </div>
                        ))
                      )}
                    </div>
                  </div>
                )}

                {/* 3. BRANDING TAB (Child Customization) */}
                {activeConsoleTab === 'branding' && (
                  <div className="space-y-4">
                    <div className="flex justify-between items-center border-b border-m3-outline/10 pb-3">
                      <div>
                        <h3 className="text-xs font-black uppercase tracking-wider text-m3-on-surface">
                          Custom Branding Styles
                        </h3>
                        <p className="text-[9px] text-m3-secondary font-bold">
                          Configure tenant overrides for logos and themes.
                        </p>
                      </div>
                    </div>

                    <form onSubmit={handleUpdateBranding} className="space-y-3">
                      <M3TextField
                        label="Portal Welcome Title"
                        required
                        value={brandTitle}
                        onChange={(e) => setBrandTitle(e.target.value)}
                        placeholder="e.g. Acme Employee Hub"
                      />

                      <M3TextField
                        label="Primary Color Token (HSL / Hex)"
                        required
                        value={brandColor}
                        onChange={(e) => setBrandColor(e.target.value)}
                        placeholder="e.g. HSL(200, 70%, 40%)"
                      />

                      <M3TextField
                        label="Logo Asset URL"
                        value={brandLogo}
                        onChange={(e) => setBrandLogo(e.target.value)}
                        placeholder="e.g. https://logo.domain.com/acme.png"
                      />

                      {brandLogo && (
                        <div className="p-3 bg-m3-surface-container/60 rounded-xl border border-m3-outline/25 flex items-center justify-between gap-4">
                          <span className="text-[9px] uppercase tracking-wider text-m3-secondary font-black">Logo Preview</span>
                          <img src={brandLogo} alt="Logo preview" className="h-6 w-auto object-contain rounded border border-m3-outline/30 bg-white p-0.5" onError={(e) => { (e.target as any).style.display = 'none'; }} />
                        </div>
                      )}

                      <M3Button
                        variant="filled"
                        type="submit"
                        className="w-full text-[9px] py-1.5 h-8.5 font-black uppercase tracking-wider"
                      >
                        Apply Branding Styles
                      </M3Button>
                    </form>
                  </div>
                )}

              </M3Card>

            </div>
          ) : (
            <M3Card variant="elevated" className="p-6 text-center text-xs text-m3-secondary border border-m3-outline/20 bg-m3-surface-container/10">
              Select a tenant block in the maintenance panel to load details and active branches list.
            </M3Card>
          )}
        </div>

      </div>

      {/* Register Tenant Modal Form */}
      <TenantForm
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        onSuccess={handleCreateSuccess}
      />
    </div>
  );
};

export default TenantDashboardScreen;

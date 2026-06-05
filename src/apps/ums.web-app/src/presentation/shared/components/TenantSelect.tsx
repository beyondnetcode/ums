import React, { useState, useRef, useEffect } from 'react';
import { ChevronDown, Search, Building2, X } from 'lucide-react';

export interface TenantOption {
  id: string;
  code: string;
  name: string;
}

interface TenantSelectProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
  tenants: TenantOption[];
  placeholder?: string;
  error?: string;
}

export const TenantSelect: React.FC<TenantSelectProps> = ({
  label,
  value,
  onChange,
  tenants,
  placeholder = 'Buscar tenant...',
  error,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [search, setSearch] = useState('');
  const inputRef = useRef<HTMLInputElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const selectedTenant = tenants.find(t => t.id === value);

  const filteredTenants = tenants.filter(
    t =>
      t.code.toLowerCase().includes(search.toLowerCase()) ||
      t.name.toLowerCase().includes(search.toLowerCase())
  );

  useEffect(() => {
    if (isOpen && inputRef.current) {
      inputRef.current.focus();
    }
  }, [isOpen]);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
        setSearch('');
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleSelect = (tenantId: string) => {
    onChange(tenantId);
    setIsOpen(false);
    setSearch('');
  };

  const handleClear = (e: React.MouseEvent) => {
    e.stopPropagation();
    onChange('');
  };

  return (
    <div ref={containerRef} className="relative w-full mb-4">
      <label className="block text-[11px] font-bold text-m3-primary uppercase tracking-wider mb-2 ml-1">
        {label}
      </label>

      <div
        onClick={() => setIsOpen(!isOpen)}
        className={`
          relative w-full h-14 rounded-[4px] border cursor-pointer
          bg-m3-surface-container/30 dark:bg-m3-surface-container/20
          transition-colors duration-150
          ${isOpen ? 'border-2 border-m3-primary' : error ? 'border border-m3-error' : 'border border-m3-outline hover:border-m3-on-surface'}
        `}
      >
        <div className="flex items-center justify-between h-full px-4">
          {selectedTenant ? (
            <div className="flex items-center gap-2 min-w-0">
              <Building2 className="w-4 h-4 text-m3-primary flex-shrink-0" />
              <span className="text-sm text-m3-on-surface truncate">{selectedTenant.code}</span>
              <span className="text-xs text-m3-secondary truncate">- {selectedTenant.name}</span>
            </div>
          ) : (
            <span className="text-sm text-m3-secondary">Seleccionar tenant...</span>
          )}

          <div className="flex items-center gap-1 flex-shrink-0">
            {selectedTenant && (
              <button
                type="button"
                onClick={handleClear}
                className="p-1 rounded-full hover:bg-m3-surface-container/50 text-m3-secondary"
              >
                <X className="w-4 h-4" />
              </button>
            )}
            <ChevronDown
              className={`w-4 h-4 text-m3-secondary transition-transform ${isOpen ? 'rotate-180' : ''}`}
            />
          </div>
        </div>
      </div>

      {isOpen && (
        <div className="absolute z-50 w-full mt-1 bg-m3-surface-container/95 backdrop-blur-sm border border-m3-outline/50 rounded-xl shadow-lg overflow-hidden">
          <div className="p-2 border-b border-m3-outline/25">
            <div className="flex items-center gap-2 px-3 py-2 rounded-lg bg-m3-surface-container/30">
              <Search className="w-4 h-4 text-m3-secondary" />
              <input
                ref={inputRef}
                type="text"
                value={search}
                onChange={e => setSearch(e.target.value)}
                placeholder={placeholder}
                className="flex-1 bg-transparent text-sm text-m3-on-surface outline-none placeholder:text-m3-secondary"
              />
            </div>
          </div>

          <div className="max-h-64 overflow-y-auto">
            {filteredTenants.length === 0 ? (
              <div className="px-4 py-3 text-sm text-m3-secondary text-center">
                No se encontraron tenants
              </div>
            ) : (
              filteredTenants.map(tenant => (
                <button
                  key={tenant.id}
                  type="button"
                  onClick={() => handleSelect(tenant.id)}
                  className={`
                    w-full px-4 py-3 flex items-center gap-3 text-left transition-colors
                    ${tenant.id === value ? 'bg-m3-primary-container/50' : 'hover:bg-m3-surface-container/50'}
                  `}
                >
                  <Building2 className="w-4 h-4 text-m3-primary flex-shrink-0" />
                  <div className="min-w-0 flex-1">
                    <span className="text-sm font-semibold text-m3-on-surface block">
                      {tenant.code}
                    </span>
                    <span className="text-xs text-m3-secondary block truncate">{tenant.name}</span>
                  </div>
                  {tenant.id === value && (
                    <div className="w-2 h-2 rounded-full bg-m3-primary flex-shrink-0" />
                  )}
                </button>
              ))
            )}
          </div>

          <div className="p-2 border-t border-m3-outline/25 text-[10px] text-m3-secondary text-center">
            {filteredTenants.length} tenant{filteredTenants.length !== 1 ? 's' : ''} encontrado
            {filteredTenants.length !== 1 ? 's' : ''}
          </div>
        </div>
      )}

      {error && <span className="block text-xs mt-1 ml-4 text-m3-error">{error}</span>}
    </div>
  );
};

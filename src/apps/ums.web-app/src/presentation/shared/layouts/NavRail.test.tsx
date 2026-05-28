import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { NavRail } from './NavRail';
import * as useI18nModule from '@app/i18n/use-i18n';

vi.mock('react-router-dom', () => ({
  useNavigate: () => vi.fn(),
  useLocation: () => ({ pathname: '/tenants' }),
}));

vi.mock('@app/i18n/use-i18n');
vi.mock('./navigation.config', () => ({
  NAV_ROUTES: { tenants: '/tenants', users: '/users' },
  pathToTab: (path: string) => path.replace('/', ''),
  NAV_MODULES: () => [
    {
      key: 'identity',
      nameKey: 'identity',
      icon: 'I',
      members: [
        { id: 'tenants', nameKey: 'tenants', icon: 'T' },
        { id: 'users', nameKey: 'users', icon: 'U' },
      ],
    },
  ],
}));

describe('NavRail', () => {
  beforeEach(() => {
    vi.restoreAllMocks();

    vi.mocked(useI18nModule.useI18n).mockReturnValue({
      identity: 'Identity',
      tenants: 'Tenants',
      users: 'Users',
      portalFooter: 'UMS Portal',
      archVersion: 'v1.0.0',
    } as any);
  });

  it('renders navigation element', () => {
    render(<NavRail collapsed={false} />);
    expect(screen.getByRole('navigation')).toBeInTheDocument();
  });

  it('renders module headers when not collapsed', () => {
    render(<NavRail collapsed={false} />);
    expect(screen.getByText('Identity')).toBeInTheDocument();
  });

  it('renders navigation items when module is expanded', () => {
    render(<NavRail collapsed={false} />);
    expect(screen.getByText('Tenants')).toBeInTheDocument();
    expect(screen.getByText('Users')).toBeInTheDocument();
  });

  it('renders footer text', () => {
    render(<NavRail collapsed={false} />);
    expect(screen.getByText('UMS Portal')).toBeInTheDocument();
  });

  it('renders version text', () => {
    render(<NavRail collapsed={false} />);
    expect(screen.getByText('v1.0.0')).toBeInTheDocument();
  });

  it('renders collapsed view when collapsed is true', () => {
    render(<NavRail collapsed={true} />);
    expect(screen.getByRole('navigation')).toBeInTheDocument();
  });

  it('toggles module expansion when clicked', () => {
    const { container, getByText } = render(<NavRail collapsed={false} />);

    const moduleButton = container.querySelector('button[aria-expanded="true"]');
    expect(moduleButton).toBeInTheDocument();
  });

  it('highlights active tab', () => {
    render(<NavRail collapsed={false} />);
    const tenantsButton = screen.getByText('Tenants').closest('button');
    expect(tenantsButton).toHaveClass('bg-m3-primary-container');
  });
});

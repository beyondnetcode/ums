import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MasterDetailLayout } from './MasterDetailLayout';

describe('MasterDetailLayout', () => {
  it('renders master content', () => {
    render(
      <MasterDetailLayout
        master={<div data-testid="master">Master</div>}
        detail={<div data-testid="detail">Detail</div>}
      />
    );
    expect(screen.getByTestId('master')).toBeInTheDocument();
  });

  it('renders detail content', () => {
    render(
      <MasterDetailLayout
        master={<div>Master</div>}
        detail={<div data-testid="detail">Detail</div>}
      />
    );
    expect(screen.getByTestId('detail')).toBeInTheDocument();
  });

  it('renders overlay when provided', () => {
    render(
      <MasterDetailLayout
        master={<div>Master</div>}
        detail={<div>Detail</div>}
        overlay={<div data-testid="overlay">Overlay</div>}
      />
    );
    expect(screen.getByTestId('overlay')).toBeInTheDocument();
  });

  it('has splitter with accessible label', () => {
    render(
      <MasterDetailLayout
        master={<div>Master</div>}
        detail={<div>Detail</div>}
        splitterLabel="Custom label"
      />
    );
    const separator = screen.getByRole('separator');
    expect(separator).toHaveAttribute('aria-label', 'Custom label');
  });

  it('has default splitter label', () => {
    render(<MasterDetailLayout master={<div>Master</div>} detail={<div>Detail</div>} />);
    const separator = screen.getByRole('separator');
    expect(separator).toHaveAttribute('aria-label', 'Resize detail panel');
  });

  it('has separator role', () => {
    render(<MasterDetailLayout master={<div>Master</div>} detail={<div>Detail</div>} />);
    expect(screen.getByRole('separator')).toHaveAttribute('role', 'separator');
  });

  it('has keyboard focus support', () => {
    render(<MasterDetailLayout master={<div>Master</div>} detail={<div>Detail</div>} />);
    const separator = screen.getByRole('separator');
    expect(separator).toHaveAttribute('tabIndex', '0');
  });
});

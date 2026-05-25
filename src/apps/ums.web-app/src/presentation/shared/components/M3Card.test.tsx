import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { M3Card } from './M3Card';
import userEvent from '@testing-library/user-event';

describe('M3Card Component', () => {
  it('renders children correctly', () => {
    render(<M3Card>Test Content</M3Card>);
    expect(screen.getByText('Test Content')).toBeInTheDocument();
  });

  it('handles click events when clickable', async () => {
    const handleClick = vi.fn();
    const user = userEvent.setup();
    render(<M3Card onClick={handleClick}>Click Me</M3Card>);
    
    await user.click(screen.getByText('Click Me'));
    expect(handleClick).toHaveBeenCalledTimes(1);
  });
});

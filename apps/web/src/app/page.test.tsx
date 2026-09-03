import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import Page from './page';

describe('HomePage', () => {
  it('renders the platform title', () => {
    render(<Page />);

    expect(screen.getByRole('heading', { name: /trauma-informed community platform/i })).toBeInTheDocument();
  });
});

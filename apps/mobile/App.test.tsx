import renderer from 'react-test-renderer';
import App from './App';

describe('App', () => {
  it('renders the app title', () => {
    const tree = renderer.create(<App />).toJSON();
    expect(tree).toMatchSnapshot();
  });
});

module.exports = {
  preset: 'react-native',
  testEnvironment: 'node',
  transformIgnorePatterns: ['/node_modules/(?!((jest-)?react-native|@react-native|expo(nent)?|@expo(nent)?/.*|react-navigation|@react-navigation/.*|@sentry/react-native|native-base|react-native-svg))'],
};

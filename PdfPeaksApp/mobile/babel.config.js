module.exports = {
  presets: ['module:@react-native/babel-preset'],
  plugins: [
    'react-native-reanimated/plugin',
    [
      'module-resolver',
      {
        root: ['./'],
        alias: {
          '@': './src',
          '@components': './src/components',
          '@screens': './src/screens',
          '@services': './src/services',
          '@store': './src/store',
          '@utils': './src/utils',
          '@hooks': './src/hooks',
          '@types': './src/types',
          '@constants': './src/constants',
          '@navigation': './src/navigation',
        },
      },
    ],
  ],
};

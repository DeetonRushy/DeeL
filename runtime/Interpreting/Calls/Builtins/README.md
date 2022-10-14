# Builtin functions

### `relative`

Convert a relative path to an absolute path.

```
'paths' = {
    'assets': relative('/assets'),
    'fonts': relative('/assets/fonts')
};
```

This is to avoid working out the absolute path at runtime. This is also cross platform,
meaning the path would be `C:\Path\To\Project\assets` on windows, but on linux it would be
`/usr/code/project/assets`.
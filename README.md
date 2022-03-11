# Git Release Deployer Readme

A background service that regularly checks and deploys your git releases or assets to your desired local web servers.

## How to use

Create a ```config.xml``` file with the help of this template:

```
<?xml version="1.0" encoding="UTF-8"?>
<serviceConfig>
  <gitRepo username="[YOUR_GIT_USERNAME]" repo="[YOUR_GIT_REPO_NAME]">
    <token>[YOUR_GIT_ACCESS_TOKEN]</token>
    <deploy type="iis">[IIS_SITE_OR_SITE/VIRTUAL_DIRECTORY_PATH]></deploy>
	  <checkIntervalMinutes>[INTEGER_VALUE]</checkIntervalMinutes>
  </gitRepo>
</serviceConfig>
```

Place this file in the same directory as the service executable.
Then, just register the service i.e. on Windows with this command:

```
SC CREATE "Windows Bouncer Service" binpath= "C:\Path\to\ReleaseDeployerService.exe"
```

## Features

*  Customizable checking interval
*  Currently IIS & Windows only
*  Linux edition planned for next release

## Release Notes

All releases are made available [here](https://github.com/hakanyildizhan/git-release-deployer/releases).
## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

##  RoATP Roatp Oversight

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status%2FApprenticeships%20Providers%2Fdas-roatp-oversight?repoName=SkillsFundingAgency%2Fdas-roatp-oversight&branchName=master)](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_build/latest?definitionId=2152&repoName=SkillsFundingAgency%2Fdas-roatp-oversight&branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-roatp-oversight&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SkillsFundingAgency_das-roatp-oversight)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

## 🚀 Installation

### Pre-Requisites
* A clone of this repository
* Visual Studio or similar IDE
* A storage emulator (for example Azurite)

### Dependencies
* Apply Service (https://github.com/SkillsFundingAgency/das-apply-service)
* Admin Service (https://github.com/SkillsFundingAgency/das-admin-service)
* Assessor Service (https://github.com/SkillsFundingAgency/das-assessor-service)
* Roatp Service (https://github.com/SkillsFundingAgency/das-roatp-service)

### Config
* Grab the das-roatp-oversight configuration json file from [das-employer-config](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-roatp-oversight/SFA.DAS.RoatpOversight.json)
* Create a Configuration table in your (Development) local Azure Storage account.
* Add a row to the Configuration table with fields: PartitionKey: LOCAL, RowKey: SFA.DAS.RoatpOversight_1.0, Data: {The contents of the local config json file}.
* Alter the SqlConnectionString value in the json to point to your database.

## Technologies
* .Net 10.0
* Refit
# ![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png) Digital Apprenticeships Service
##  RoATP Roatp Oversight
# das-roatp-oversight



#### Requirements

- Install [.NET Core 2.2](https://www.microsoft.com/net/download)
- Install [Azure Storage Emulator](https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409) (Make sure you are on v5.3)
- Install [Azure Storage Explorer](http://storageexplorer.com/)
- Install the editor of your choice:
  - [Jetbrains Rider](https://www.jetbrains.com/rider/)
  - [Visual Studio Code](https://code.visualstudio.com/)
  - [Visual Studio](https://visualstudio.microsoft.com/)

#### Setup

- Clone this repository


##### Code

- Create a Configuration table in your (Development) local Azure Storage account.
- Add a row to the Configuration table with fields: PartitionKey: LOCAL, RowKey: SFA.DAS.RoatpOversite_1.0, Data: {
	"SessionRedisConnectionString": "localhost",
	"StaffAuthentication": {
		"WtRealm": "https://localhost:45667",
		"MetadataAddress": "https://adfs.preprod.skillsfunding.service.gov.uk/FederationMetadata/2007-06/FederationMetadata.xml"
	}
}.

### Running the code

- `dotnet run` the following projects:
  - SFA.DAS.RoatpOversight.Web
- Navigate to (https://localhost:45667) and you should see the start page.

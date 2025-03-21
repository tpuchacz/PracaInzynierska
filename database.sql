USE [inz]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[software]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[software](
	[softwareId] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](50) NOT NULL,
	[websiteLink] [varchar](255) NOT NULL,
	[downloadLink] [varchar](255) NOT NULL,
	[companyName] [varchar](255) NULL,
	[currentVersion] [varchar](100) NULL,
	[updateDate] [date] NULL,
	[category] [varchar](100) NOT NULL,
	[parameterSilent] [varchar](20) NULL,
	[parameterDir] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[softwareId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[telemetryData]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[telemetryData](
	[telemetryId] [int] NOT NULL,
	[downloadCount] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[telemetryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[templates]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[templates](
	[templateName] [varchar](50) NOT NULL,
	[procedureName] [varchar](50) NOT NULL,
	[templateId] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]
END
GO
SET IDENTITY_INSERT [dbo].[software] ON 

INSERT [dbo].[software] ([softwareId], [name], [websiteLink], [downloadLink], [companyName], [currentVersion], [updateDate], [category], [parameterSilent], [parameterDir]) VALUES (1, N'WinRar', N'https://www.win-rar.com', N'https://www.win-rar.com/fileadmin/winrar-versions/winrar/winrar-x64-710.exe', N'win.rar GmbH', N'7.1.0', CAST(N'2025-02-25' AS Date), N'Zarządzanie plikami', N'/S', N'/D')
INSERT [dbo].[software] ([softwareId], [name], [websiteLink], [downloadLink], [companyName], [currentVersion], [updateDate], [category], [parameterSilent], [parameterDir]) VALUES (2, N'7-Zip', N'https://www.7-zip.org/', N'https://www.7-zip.org/a/7z2409-x64.exe', N'Igor Pavlov', N'24.09', CAST(N'2025-02-06' AS Date), N'Zarządzanie plikami', N'/S', N'/D=')
INSERT [dbo].[software] ([softwareId], [name], [websiteLink], [downloadLink], [companyName], [currentVersion], [updateDate], [category], [parameterSilent], [parameterDir]) VALUES (3, N'Notepad++', N'https://notepad-plus-plus.org/', N'https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.7.6/npp.8.7.6.Installer.x64.exe', N'Don Ho', N'8.7.6', CAST(N'2025-02-06' AS Date), N'Edytor tekstu', N'/S', N'/D=')
INSERT [dbo].[software] ([softwareId], [name], [websiteLink], [downloadLink], [companyName], [currentVersion], [updateDate], [category], [parameterSilent], [parameterDir]) VALUES (4, N'qBittorrent', N'https://www.qbittorrent.org', N'https://sourceforge.net/projects/qbittorrent/files/qbittorrent-win32/qbittorrent-5.0.3/qbittorrent_5.0.3_x64_setup.exe/download', N'The qBittorrent project', N'5.0.3', CAST(N'2025-02-15' AS Date), N'Udostępnianie plików', N'/S', N'/D=')
INSERT [dbo].[software] ([softwareId], [name], [websiteLink], [downloadLink], [companyName], [currentVersion], [updateDate], [category], [parameterSilent], [parameterDir]) VALUES (1006, N'Audacity', N'https://www.audacityteam.org', N'https://github.com/audacity/audacity/releases/download/Audacity-3.7.1/audacity-win-3.7.1-64bit.exe', N'Muse Group', N'3.7.1', CAST(N'2025-02-24' AS Date), N'Odtwarzacze multimediów', N'/VERYSILENT', N'/DIR=')
INSERT [dbo].[software] ([softwareId], [name], [websiteLink], [downloadLink], [companyName], [currentVersion], [updateDate], [category], [parameterSilent], [parameterDir]) VALUES (100007, N'CPU-Z', N'https://www.cpuid.com/softwares/cpu-z.html', N'https://download.cpuid.com/cpu-z/cpu-z_2.14-en.exe', N'CPUID', N'2.14', CAST(N'2025-03-05' AS Date), N'Monitorowanie zasobów', N'/VERYSILENT', N'/DIR=')
INSERT [dbo].[software] ([softwareId], [name], [websiteLink], [downloadLink], [companyName], [currentVersion], [updateDate], [category], [parameterSilent], [parameterDir]) VALUES (100009, N'Core Temp', N'https://www.alcpu.com/CoreTemp/', N'https://www.alcpu.com/CoreTemp/Core-Temp-setup-v1.18.1.0.exe', N'Arthur Liberman', N'1.18.1', CAST(N'2025-03-05' AS Date), N'Monitorowanie zasobów', N'/VERYSILENT', N'/DIR=')
SET IDENTITY_INSERT [dbo].[software] OFF
GO
INSERT [dbo].[telemetryData] ([telemetryId], [downloadCount]) VALUES (1, 6)
INSERT [dbo].[telemetryData] ([telemetryId], [downloadCount]) VALUES (2, 15)
INSERT [dbo].[telemetryData] ([telemetryId], [downloadCount]) VALUES (3, 3)
INSERT [dbo].[telemetryData] ([telemetryId], [downloadCount]) VALUES (4, 24)
INSERT [dbo].[telemetryData] ([telemetryId], [downloadCount]) VALUES (1006, 3)
INSERT [dbo].[telemetryData] ([telemetryId], [downloadCount]) VALUES (100007, 2)
INSERT [dbo].[telemetryData] ([telemetryId], [downloadCount]) VALUES (100009, 4)
GO
SET IDENTITY_INSERT [dbo].[templates] ON 

INSERT [dbo].[templates] ([templateName], [procedureName], [templateId]) VALUES (N'Najpopularniejsze 5 aplikacji', N'GetTopDownloads', 1)
INSERT [dbo].[templates] ([templateName], [procedureName], [templateId]) VALUES (N'Najpopularniejsze ostatnio zaktualizowane programy', N'GetTopLatestUpdated', 3)
SET IDENTITY_INSERT [dbo].[templates] OFF
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[telemetryData_software_FK]') AND parent_object_id = OBJECT_ID(N'[dbo].[telemetryData]'))
ALTER TABLE [dbo].[telemetryData]  WITH CHECK ADD  CONSTRAINT [telemetryData_software_FK] FOREIGN KEY([telemetryId])
REFERENCES [dbo].[software] ([softwareId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[telemetryData_software_FK]') AND parent_object_id = OBJECT_ID(N'[dbo].[telemetryData]'))
ALTER TABLE [dbo].[telemetryData] CHECK CONSTRAINT [telemetryData_software_FK]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTopDownloads]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetTopDownloads] AS' 
END
GO
ALTER PROCEDURE [dbo].[GetTopDownloads]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP 5 s.softwareId, s.[name], s.[websiteLink], s.[downloadLink], s.[companyName], s.[currentVersion], s.[updateDate], s.[category], s.[parameterSilent], s.[parameterDir], t.downloadCount
	FROM ((dbo.software as s INNER JOIN dbo.telemetryData as t ON s.softwareId = t.telemetryId)) ORDER BY t.downloadCount DESC
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTopLatestUpdated]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetTopLatestUpdated] AS' 
END
GO
ALTER PROCEDURE [dbo].[GetTopLatestUpdated]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP 3 s.softwareId, s.[name], s.[websiteLink], s.[downloadLink], s.[companyName], s.[currentVersion], s.[updateDate], s.[category], s.[parameterSilent], s.[parameterDir], t.downloadCount
	FROM ((dbo.software as s INNER JOIN dbo.telemetryData as t ON s.softwareId = t.telemetryId)) WHERE DATEDIFF(day,s.updateDate, GETDATE())<=7 ORDER BY t.downloadCount DESC
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[createTelemetryData]'))
EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [dbo].[createTelemetryData]
ON [dbo].[software]
AFTER INSERT
AS 
BEGIN
SET NOCOUNT ON;
    INSERT INTO dbo.telemetryData (telemetryId, downloadCount)
        SELECT i.softwareId, 0
        FROM INSERTED i
END
' 
GO
ALTER TABLE [dbo].[software] ENABLE TRIGGER [createTelemetryData]
GO
IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'telemetryData', N'CONSTRAINT',N'telemetryData_software_FK'))
	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'adf' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'telemetryData', @level2type=N'CONSTRAINT',@level2name=N'telemetryData_software_FK'
GO

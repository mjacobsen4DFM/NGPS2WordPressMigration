
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 12/02/2013 14:00:46
-- Generated from EDMX file: C:\apps\ArticleDataServiceMigration\DataModels\ArticleDataDbModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [ArticleDataDb];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Exception_SessionError]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Exception] DROP CONSTRAINT [FK_Exception_SessionError];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[article]', 'U') IS NOT NULL
    DROP TABLE [dbo].[article];
GO
IF OBJECT_ID(N'[dbo].[article_lock]', 'U') IS NOT NULL
    DROP TABLE [dbo].[article_lock];
GO
IF OBJECT_ID(N'[dbo].[articleEditOrg]', 'U') IS NOT NULL
    DROP TABLE [dbo].[articleEditOrg];
GO
IF OBJECT_ID(N'[dbo].[asset]', 'U') IS NOT NULL
    DROP TABLE [dbo].[asset];
GO
IF OBJECT_ID(N'[dbo].[Exception]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Exception];
GO
IF OBJECT_ID(N'[dbo].[htmlHoldRules]', 'U') IS NOT NULL
    DROP TABLE [dbo].[htmlHoldRules];
GO
IF OBJECT_ID(N'[dbo].[Image]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Image];
GO
IF OBJECT_ID(N'[dbo].[image_tracker]', 'U') IS NOT NULL
    DROP TABLE [dbo].[image_tracker];
GO
IF OBJECT_ID(N'[ArticleDataDbModelStoreContainer].[logstatements]', 'U') IS NOT NULL
    DROP TABLE [ArticleDataDbModelStoreContainer].[logstatements];
GO
IF OBJECT_ID(N'[ArticleDataDbModelStoreContainer].[MigrationError]', 'U') IS NOT NULL
    DROP TABLE [ArticleDataDbModelStoreContainer].[MigrationError];
GO
IF OBJECT_ID(N'[dbo].[profile]', 'U') IS NOT NULL
    DROP TABLE [dbo].[profile];
GO
IF OBJECT_ID(N'[ArticleDataDbModelStoreContainer].[saxo_article]', 'U') IS NOT NULL
    DROP TABLE [ArticleDataDbModelStoreContainer].[saxo_article];
GO
IF OBJECT_ID(N'[ArticleDataDbModelStoreContainer].[saxo_gallery]', 'U') IS NOT NULL
    DROP TABLE [ArticleDataDbModelStoreContainer].[saxo_gallery];
GO
IF OBJECT_ID(N'[ArticleDataDbModelStoreContainer].[saxo_image]', 'U') IS NOT NULL
    DROP TABLE [ArticleDataDbModelStoreContainer].[saxo_image];
GO
IF OBJECT_ID(N'[dbo].[saxo_pubMap]', 'U') IS NOT NULL
    DROP TABLE [dbo].[saxo_pubMap];
GO
IF OBJECT_ID(N'[dbo].[sectionAnchorMap]', 'U') IS NOT NULL
    DROP TABLE [dbo].[sectionAnchorMap];
GO
IF OBJECT_ID(N'[dbo].[SessionError]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SessionError];
GO
IF OBJECT_ID(N'[ArticleDataDbModelStoreContainer].[tracksession]', 'U') IS NOT NULL
    DROP TABLE [ArticleDataDbModelStoreContainer].[tracksession];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'articles'
CREATE TABLE [dbo].[articles] (
    [siteid] varchar(10)  NOT NULL,
    [article_uid] varchar(32)  NOT NULL,
    [category] varchar(50)  NULL,
    [anchor] varchar(50)  NULL,
    [startdate] datetime  NOT NULL,
    [enddate] datetime  NULL,
    [heading] varchar(max)  NULL,
    [body] varchar(max)  NULL,
    [relatedArticles] varchar(max)  NULL,
    [summary] varchar(max)  NULL,
    [byline] varchar(max)  NULL,
    [subtitle] varchar(max)  NULL,
    [seodescription] varchar(max)  NULL,
    [keyword] varchar(max)  NULL,
    [imagecount] int  NULL,
    [origsite] varchar(max)  NULL,
    [displaygroup] varchar(max)  NULL
);
GO

-- Creating table 'article_lock'
CREATE TABLE [dbo].[article_lock] (
    [article_uid] varchar(32)  NOT NULL,
    [locked] varchar(1)  NOT NULL,
    [username] varchar(50)  NULL
);
GO

-- Creating table 'articleEditOrgs'
CREATE TABLE [dbo].[articleEditOrgs] (
    [article_uid] varchar(32)  NOT NULL,
    [heading] varchar(max)  NULL,
    [summary] varchar(max)  NULL,
    [byline] varchar(max)  NULL,
    [body] varchar(max)  NULL,
    [ts] datetime  NOT NULL
);
GO

-- Creating table 'assets'
CREATE TABLE [dbo].[assets] (
    [article_uid] varchar(32)  NOT NULL,
    [asset_uid] varchar(32)  NOT NULL,
    [asset_type] int  NOT NULL
);
GO

-- Creating table 'Exceptions'
CREATE TABLE [dbo].[Exceptions] (
    [ExceptionID] int IDENTITY(1,1) NOT NULL,
    [ExceptionLevel] int  NOT NULL,
    [SessionErrorID] int  NOT NULL,
    [Source] varchar(200)  NULL,
    [StackTrace] varchar(4000)  NULL,
    [Message] varchar(1000)  NULL,
    [Machine] varchar(50)  NULL,
    [TargetSite] varchar(100)  NULL,
    [ts] datetime  NULL
);
GO

-- Creating table 'htmlHoldRules'
CREATE TABLE [dbo].[htmlHoldRules] (
    [id] int IDENTITY(1,1) NOT NULL,
    [htmltag] varchar(50)  NULL
);
GO

-- Creating table 'Images'
CREATE TABLE [dbo].[Images] (
    [asset_uid] varchar(32)  NOT NULL,
    [imagepath] varchar(255)  NULL,
    [position] varchar(50)  NULL,
    [width] int  NULL,
    [height] int  NULL,
    [media_type] varchar(50)  NULL,
    [caption] varchar(max)  NULL,
    [filesize] int  NULL
);
GO

-- Creating table 'image_tracker'
CREATE TABLE [dbo].[image_tracker] (
    [article_uid] varchar(32)  NOT NULL,
    [asset_uid] varchar(32)  NOT NULL,
    [siteid] varchar(10)  NOT NULL,
    [soft_delete] varchar(1)  NOT NULL
);
GO

-- Creating table 'logstatements'
CREATE TABLE [dbo].[logstatements] (
    [article_uid] varchar(32)  NOT NULL,
    [statement] varchar(max)  NOT NULL,
    [ts] datetime  NOT NULL
);
GO

-- Creating table 'MigrationErrors'
CREATE TABLE [dbo].[MigrationErrors] (
    [article_uid] varchar(32)  NOT NULL,
    [myMessage] varchar(max)  NULL,
    [ts] datetime  NULL,
    [errortype] int  NULL
);
GO

-- Creating table 'profiles'
CREATE TABLE [dbo].[profiles] (
    [id] int  NOT NULL,
    [uri] varchar(max)  NOT NULL,
    [childcount] int  NOT NULL,
    [childrenuri] varchar(max)  NULL,
    [fieldname] varchar(max)  NOT NULL,
    [treelevel] int  NOT NULL,
    [parentid] int  NOT NULL,
    [rootid] int  NOT NULL,
    [realid] int  NULL
);
GO

-- Creating table 'saxo_article'
CREATE TABLE [dbo].[saxo_article] (
    [article_uid] varchar(32)  NOT NULL,
    [siteid] varchar(50)  NOT NULL,
    [destination_siteid] varchar(5)  NULL,
    [xmldata] varchar(max)  NOT NULL,
    [viewuri] varchar(max)  NULL,
    [storyurl] varchar(max)  NULL
);
GO

-- Creating table 'saxo_categoryMap'
CREATE TABLE [dbo].[saxo_categoryMap] (
    [id] int IDENTITY(1,1) NOT NULL,
    [category] varchar(50)  NOT NULL,
    [saxo_category] varchar(50)  NOT NULL,
    [taxonomyword] int  NOT NULL
);
GO

-- Creating table 'saxo_gallery'
CREATE TABLE [dbo].[saxo_gallery] (
    [destination_siteid] varchar(5)  NULL,
    [article_uid] varchar(32)  NOT NULL,
    [gallery_uid] varchar(max)  NOT NULL,
    [similarity] float  NULL,
    [edit] varchar(1)  NULL
);
GO

-- Creating table 'saxo_image'
CREATE TABLE [dbo].[saxo_image] (
    [asset_uid] varchar(32)  NOT NULL,
    [url] varchar(max)  NOT NULL,
    [destination_siteid] varchar(5)  NULL
);
GO

-- Creating table 'saxo_pubMap'
CREATE TABLE [dbo].[saxo_pubMap] (
    [siteid] int  NOT NULL,
    [pubname] varchar(50)  NOT NULL,
    [destination_siteid] varchar(50)  NOT NULL,
    [OWStarget] varchar(255)  NULL,
    [OWSViewSearch] varchar(255)  NULL,
    [OWSViewReplace] varchar(255)  NULL,
    [saxoname] varchar(100)  NULL,
    [taxonomyPubId] int  NULL,
    [sectionmapcomplete] varchar(1)  NULL
);
GO

-- Creating table 'sectionAnchorMaps'
CREATE TABLE [dbo].[sectionAnchorMaps] (
    [id] int IDENTITY(1,1) NOT NULL,
    [siteid] int  NOT NULL,
    [sectionAnchor] varchar(128)  NOT NULL,
    [sectionName] varchar(128)  NULL,
    [ProfileId] int  NOT NULL,
    [ProfileId2] int  NULL
);
GO

-- Creating table 'SessionErrors'
CREATE TABLE [dbo].[SessionErrors] (
    [SessionErrorID] int IDENTITY(1,1) NOT NULL,
    [SID] varchar(50)  NOT NULL,
    [RequestMethod] varchar(5)  NULL,
    [ServerPort] int  NULL,
    [HTTPS] varchar(3)  NULL,
    [LocalAddr] varchar(15)  NULL,
    [HostAddress] varchar(15)  NULL,
    [UserAgent] varchar(255)  NULL,
    [URL] varchar(400)  NULL,
    [CustomerRefID] varchar(20)  NULL,
    [FormData] varchar(2000)  NULL,
    [AllHTTP] varchar(2000)  NULL,
    [InsertDate] datetime  NOT NULL,
    [IsCookieLess] bit  NULL,
    [IsNewSession] bit  NULL
);
GO

-- Creating table 'tracksessions'
CREATE TABLE [dbo].[tracksessions] (
    [sid] varchar(40)  NOT NULL,
    [xmldocument] nvarchar(max)  NOT NULL,
    [ts] datetime  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [article_uid] in table 'articles'
ALTER TABLE [dbo].[articles]
ADD CONSTRAINT [PK_articles]
    PRIMARY KEY CLUSTERED ([article_uid] ASC);
GO

-- Creating primary key on [article_uid] in table 'article_lock'
ALTER TABLE [dbo].[article_lock]
ADD CONSTRAINT [PK_article_lock]
    PRIMARY KEY CLUSTERED ([article_uid] ASC);
GO

-- Creating primary key on [article_uid] in table 'articleEditOrgs'
ALTER TABLE [dbo].[articleEditOrgs]
ADD CONSTRAINT [PK_articleEditOrgs]
    PRIMARY KEY CLUSTERED ([article_uid] ASC);
GO

-- Creating primary key on [article_uid], [asset_uid] in table 'assets'
ALTER TABLE [dbo].[assets]
ADD CONSTRAINT [PK_assets]
    PRIMARY KEY CLUSTERED ([article_uid], [asset_uid] ASC);
GO

-- Creating primary key on [ExceptionID] in table 'Exceptions'
ALTER TABLE [dbo].[Exceptions]
ADD CONSTRAINT [PK_Exceptions]
    PRIMARY KEY CLUSTERED ([ExceptionID] ASC);
GO

-- Creating primary key on [id] in table 'htmlHoldRules'
ALTER TABLE [dbo].[htmlHoldRules]
ADD CONSTRAINT [PK_htmlHoldRules]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [asset_uid] in table 'Images'
ALTER TABLE [dbo].[Images]
ADD CONSTRAINT [PK_Images]
    PRIMARY KEY CLUSTERED ([asset_uid] ASC);
GO

-- Creating primary key on [article_uid], [asset_uid] in table 'image_tracker'
ALTER TABLE [dbo].[image_tracker]
ADD CONSTRAINT [PK_image_tracker]
    PRIMARY KEY CLUSTERED ([article_uid], [asset_uid] ASC);
GO

-- Creating primary key on [article_uid], [statement], [ts] in table 'logstatements'
ALTER TABLE [dbo].[logstatements]
ADD CONSTRAINT [PK_logstatements]
    PRIMARY KEY CLUSTERED ([article_uid], [statement], [ts] ASC);
GO

-- Creating primary key on [article_uid] in table 'MigrationErrors'
ALTER TABLE [dbo].[MigrationErrors]
ADD CONSTRAINT [PK_MigrationErrors]
    PRIMARY KEY CLUSTERED ([article_uid] ASC);
GO

-- Creating primary key on [id] in table 'profiles'
ALTER TABLE [dbo].[profiles]
ADD CONSTRAINT [PK_profiles]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [article_uid], [siteid], [xmldata] in table 'saxo_article'
ALTER TABLE [dbo].[saxo_article]
ADD CONSTRAINT [PK_saxo_article]
    PRIMARY KEY CLUSTERED ([article_uid], [siteid], [xmldata] ASC);
GO

-- Creating primary key on [id] in table 'saxo_categoryMap'
ALTER TABLE [dbo].[saxo_categoryMap]
ADD CONSTRAINT [PK_saxo_categoryMap]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [article_uid], [gallery_uid] in table 'saxo_gallery'
ALTER TABLE [dbo].[saxo_gallery]
ADD CONSTRAINT [PK_saxo_gallery]
    PRIMARY KEY CLUSTERED ([article_uid], [gallery_uid] ASC);
GO

-- Creating primary key on [asset_uid], [url] in table 'saxo_image'
ALTER TABLE [dbo].[saxo_image]
ADD CONSTRAINT [PK_saxo_image]
    PRIMARY KEY CLUSTERED ([asset_uid], [url] ASC);
GO

-- Creating primary key on [siteid] in table 'saxo_pubMap'
ALTER TABLE [dbo].[saxo_pubMap]
ADD CONSTRAINT [PK_saxo_pubMap]
    PRIMARY KEY CLUSTERED ([siteid] ASC);
GO

-- Creating primary key on [id] in table 'sectionAnchorMaps'
ALTER TABLE [dbo].[sectionAnchorMaps]
ADD CONSTRAINT [PK_sectionAnchorMaps]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [SessionErrorID] in table 'SessionErrors'
ALTER TABLE [dbo].[SessionErrors]
ADD CONSTRAINT [PK_SessionErrors]
    PRIMARY KEY CLUSTERED ([SessionErrorID] ASC);
GO

-- Creating primary key on [sid], [xmldocument], [ts] in table 'tracksessions'
ALTER TABLE [dbo].[tracksessions]
ADD CONSTRAINT [PK_tracksessions]
    PRIMARY KEY CLUSTERED ([sid], [xmldocument], [ts] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [SessionErrorID] in table 'Exceptions'
ALTER TABLE [dbo].[Exceptions]
ADD CONSTRAINT [FK_Exception_SessionError]
    FOREIGN KEY ([SessionErrorID])
    REFERENCES [dbo].[SessionErrors]
        ([SessionErrorID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Exception_SessionError'
CREATE INDEX [IX_FK_Exception_SessionError]
ON [dbo].[Exceptions]
    ([SessionErrorID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
CREATE TABLE [dbo].[Customers] (
    [Id]                UNIQUEIDENTIFIER           NOT NULL,
    [Name]              VARCHAR(50)                NOT NULL,
    CONSTRAINT [PK_PipeSystems] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
);

CREATE TABLE [dbo].[Invoices] (
    [Id]                UNIQUEIDENTIFIER           NOT NULL,
    [Date]              DATETIME                   NOT NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY],
);

CREATE TABLE [dbo].[InvoiceLines] (
    [Id]                UNIQUEIDENTIFIER           NOT NULL,
    [InvoiceId]         UNIQUEIDENTIFIER           NOT NULL,
    [Quantity]          INT                        NOT NULL,
    CONSTRAINT [PK_InvoiceLines] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY],
    CONSTRAINT [FK_InvoiceLines_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices] ([Id])
);



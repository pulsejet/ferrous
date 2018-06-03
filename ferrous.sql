CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" varchar(150) NOT NULL,
    "ProductVersion" varchar(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);


DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    CREATE TABLE "Building" (
        "Location" text NOT NULL,
        "DefaultCapacity" integer NOT NULL,
        "LocationFullName" text NULL,
        CONSTRAINT "PK_Building" PRIMARY KEY ("Location")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    CREATE TABLE "Contingents" (
        "ContingentLeaderNo" text NOT NULL,
        "Remark" text NULL,
        CONSTRAINT "PK_Contingents" PRIMARY KEY ("ContingentLeaderNo")
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    CREATE TABLE "Room" (
        "RoomId" serial NOT NULL,
        "Capacity" integer NOT NULL,
        "Location" text NULL,
        "LocationExtra" text NULL,
        "LockNo" text NULL,
        "Remark" text NULL,
        "RoomName" text NULL,
        "Status" integer NULL,
        CONSTRAINT "PK_Room" PRIMARY KEY ("RoomId"),
        CONSTRAINT "FK_Room_Building_Location" FOREIGN KEY ("Location") REFERENCES "Building" ("Location") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    CREATE TABLE "ContingentArrival" (
        "ContingentArrivalNo" serial NOT NULL,
        "ContingentLeaderNo" text NULL,
        "CreatedOn" timestamp without time zone NOT NULL,
        "Female" integer NULL,
        "FemaleOnSpot" integer NULL,
        "Male" integer NULL,
        "MaleOnSpot" integer NULL,
        CONSTRAINT "PK_ContingentArrival" PRIMARY KEY ("ContingentArrivalNo"),
        CONSTRAINT "FK_ContingentArrival_Contingents_ContingentLeaderNo" FOREIGN KEY ("ContingentLeaderNo") REFERENCES "Contingents" ("ContingentLeaderNo") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    CREATE TABLE "Person" (
        "Mino" text NOT NULL,
        "College" text NULL,
        "ContingentLeaderNo" text NULL,
        "Name" text NULL,
        "Phone" text NULL,
        "Email" text NULL,
        "Sex" varchar(1) NULL,
        CONSTRAINT "PK_Person" PRIMARY KEY ("Mino"),
        CONSTRAINT "FK_Person_Contingents_ContingentLeaderNo" FOREIGN KEY ("ContingentLeaderNo") REFERENCES "Contingents" ("ContingentLeaderNo") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    CREATE TABLE "RoomAllocation" (
        "Sno" serial NOT NULL,
        "ContingentArrivalNo" integer NULL,
        "ContingentLeaderNo" text NULL,
        "Partial" integer NOT NULL,
        "RoomId" integer NOT NULL,
        CONSTRAINT "PK_RoomAllocation" PRIMARY KEY ("Sno"),
        CONSTRAINT "FK_RoomAllocation_ContingentArrival_ContingentArrivalNo" FOREIGN KEY ("ContingentArrivalNo") REFERENCES "ContingentArrival" ("ContingentArrivalNo") ON DELETE RESTRICT,
        CONSTRAINT "FK_RoomAllocation_Contingents_ContingentLeaderNo" FOREIGN KEY ("ContingentLeaderNo") REFERENCES "Contingents" ("ContingentLeaderNo") ON DELETE RESTRICT,
        CONSTRAINT "FK_RoomAllocation_Room_RoomId" FOREIGN KEY ("RoomId") REFERENCES "Room" ("RoomId") ON DELETE CASCADE
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    CREATE INDEX "IX_ContingentArrival_ContingentLeaderNo" ON "ContingentArrival" ("ContingentLeaderNo");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    CREATE INDEX "IX_Person_ContingentLeaderNo" ON "Person" ("ContingentLeaderNo");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    CREATE INDEX "IX_Room_Location" ON "Room" ("Location");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    CREATE INDEX "IX_RoomAllocation_ContingentArrivalNo" ON "RoomAllocation" ("ContingentArrivalNo");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    CREATE INDEX "IX_RoomAllocation_ContingentLeaderNo" ON "RoomAllocation" ("ContingentLeaderNo");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    CREATE INDEX "IX_RoomAllocation_RoomId" ON "RoomAllocation" ("RoomId");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180414091319_Init') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180414091319_Init', '2.1.0-preview2-30571');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603073712_caperson') THEN
    ALTER TABLE "ContingentArrival" ADD "Approved" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603073712_caperson') THEN
    ALTER TABLE "ContingentArrival" ADD "Remark" text NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603073712_caperson') THEN
    CREATE TABLE "CAPerson" (
        "Sno" serial NOT NULL,
        "ContingentArrivalNavigationContingentArrivalNo" integer NULL,
        "Mino" text NULL,
        CONSTRAINT "PK_CAPerson" PRIMARY KEY ("Sno"),
        CONSTRAINT "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationConti~" FOREIGN KEY ("ContingentArrivalNavigationContingentArrivalNo") REFERENCES "ContingentArrival" ("ContingentArrivalNo") ON DELETE RESTRICT
    );
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603073712_caperson') THEN
    CREATE INDEX "IX_CAPerson_ContingentArrivalNavigationContingentArrivalNo" ON "CAPerson" ("ContingentArrivalNavigationContingentArrivalNo");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603073712_caperson') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180603073712_caperson', '2.1.0-preview2-30571');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603080245_casex') THEN
    ALTER TABLE "CAPerson" DROP CONSTRAINT "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationContingentArrivalNo";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603080245_casex') THEN
    ALTER TABLE "CAPerson" ADD "Sex" varchar(1) NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603080245_casex') THEN
    ALTER TABLE "CAPerson" ADD CONSTRAINT "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationConti~" FOREIGN KEY ("ContingentArrivalNavigationContingentArrivalNo") REFERENCES "ContingentArrival" ("ContingentArrivalNo") ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603080245_casex') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180603080245_casex', '2.1.0-preview2-30571');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603085047_allottedca') THEN
    ALTER TABLE "CAPerson" DROP CONSTRAINT "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationContingentArrivalNo";
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603085047_allottedca') THEN
    ALTER TABLE "Person" ADD "allottedCAContingentArrivalNo" integer NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603085047_allottedca') THEN
    CREATE INDEX "IX_Person_allottedCAContingentArrivalNo" ON "Person" ("allottedCAContingentArrivalNo");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603085047_allottedca') THEN
    ALTER TABLE "CAPerson" ADD CONSTRAINT "FK_CAPerson_ContingentArrival_ContingentArrivalNavigationConti~" FOREIGN KEY ("ContingentArrivalNavigationContingentArrivalNo") REFERENCES "ContingentArrival" ("ContingentArrivalNo") ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603085047_allottedca') THEN
    ALTER TABLE "Person" ADD CONSTRAINT "FK_Person_ContingentArrival_allottedCAContingentArrivalNo" FOREIGN KEY ("allottedCAContingentArrivalNo") REFERENCES "ContingentArrival" ("ContingentArrivalNo") ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20180603085047_allottedca') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20180603085047_allottedca', '2.1.0-preview2-30571');
    END IF;
END $$;

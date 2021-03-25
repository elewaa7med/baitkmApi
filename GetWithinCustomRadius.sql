USE [DB_A48685_BaitkmDb]

DROP PROCEDURE IF EXISTS [dbo].[GetWithinCustomRadius]
GO

CREATE PROCEDURE [dbo].[GetWithinCustomRadius](@lat DECIMAL(18,6), @lng DECIMAL(18,6), @radius INT)
AS
	BEGIN
		Create TABLE #value_table 
		(
			Id INT,
			Distance INT,
			Lat DECIMAL(18,6),
			Lng DECIMAL(18,6)
		)
		DECLARE @id_to_remove TABLE
		(
			Id INT
		)
		IF(@radius <> 0)
		BEGIN
			DECLARE @count INT = 0;
			INSERT INTO #value_table ([Id], [Distance], [Lat], [Lng]) (SELECT [Id], 0.0, [Lat], [Lng] FROM [dbo].[Announcements] WHERE IsDeleted <> 1);
			SET @count = (SELECT COUNT(*) FROM #value_table)
			IF(@count <> 0)
			BEGIN
				DECLARE @iterator INT = 1;
				DECLARE @orig GEOGRAPHY = GEOGRAPHY::Point(@lat, @lng, 4326);
				WHILE(@iterator <= @count)
				BEGIN
					DECLARE @current_lat DECIMAL(18,6) = NULL;
					DECLARE @current_lng DECIMAL(18,6) = NULL;
					SET @current_lat = (SELECT [Lat] FROM #value_table ORDER BY CURRENT_TIMESTAMP OFFSET (@iterator - 1) ROWS FETCH NEXT 1 ROWS ONLY);
					SET @current_lng = (SELECT [Lng] FROM #value_table ORDER BY CURRENT_TIMESTAMP OFFSET (@iterator - 1) ROWS FETCH NEXT 1 ROWS ONLY);
					DECLARE @current_point GEOGRAPHY = NULL;
					SET @current_point = GEOGRAPHY::Point(@current_lat, @current_lng, 4326);
					DECLARE @distance FLOAT = @orig.STDistance(@current_point);
					IF(@distance > @radius)
					BEGIN
						DECLARE @id INT = (SELECT [Id] FROM #value_table ORDER BY CURRENT_TIMESTAMP OFFSET (@iterator - 1) ROWS FETCH NEXT 1 ROWS ONLY);
						INSERT INTO @id_to_remove ([Id]) VALUES(@id);
						SET @iterator = @iterator + 1;
						CONTINUE;
					END
					UPDATE #value_table SET [Distance] = @distance WHERE [Id] = (SELECT [Id] FROM #value_table ORDER BY CURRENT_TIMESTAMP OFFSET (@iterator - 1) ROWS FETCH NEXT 1 ROWS ONLY)
					SET @iterator = @iterator + 1;
				END
				DECLARE @remove_count INT;
				SET @remove_count = (SELECT COUNT(*) FROM @id_to_remove);
				DECLARE @remove_iterator INT = 1;
				IF(@remove_count > 0)
				BEGIN 
					WHILE (@remove_iterator <= @remove_count)
					BEGIN
						DECLARE @remove_id INT = NULL;
						SET @remove_id = (SELECT [Id] FROM @id_to_remove ORDER BY CURRENT_TIMESTAMP OFFSET (@remove_iterator - 1) ROWS FETCH NEXT 1 ROWS ONLY);
						DELETE FROM #value_table WHERE [Id] = @remove_id;
						SET @remove_iterator = @remove_iterator + 1;
					END
				END
			END
		END
		SELECT * FROM #value_table;
	END
--DROP TABLE IF EXISTS #result_table
--GO
--CREATE TABLE #result_table
--		(
--			Day DATETIME2,
--			Duration FLOAT
--		)
--		DECLARE @counter INT = 12;
--		DECLARE @counterd INT = 1;
--		DECLARE @activity TABLE
--		(
--			UserId INT, 
--			ActivityDate DATETIME2
--		);
--		DECLARE @user_ids TABLE
--		(
--			UserId INT
--		)
--		DECLARE @user_activity TABLE
--		(
--			UserId INT, 
--			ActivityDate DATETIME2
--		);
--		DECLARE @user_duration TABLE
--		(
--			Duration FLOAT,
--			UserId INT
--		)
--		WHILE(@counterd > 0)
--		BEGIN
--			DELETE FROM @activity;
--			DELETE FROM @user_ids;
--			DELETE FROM @user_duration;
--			DECLARE @iteration_day DATETIME2 = NULL;
--			SET @iteration_day = CAST(DATEADD(Day, (13 * -1), GETDATE()) As date);
--			INSERT INTO #result_table (Day, Duration) VALUES (@iteration_day, 0);
--			DECLARE @user_count INT = NULL;
--			INSERT INTO @activity (UserId, ActivityDate) (SELECT UserId, ActivityDate
--				FROM [Statistics] Where CAST(CreatedDt as date) = @iteration_day);
--			INSERT INTO @user_ids (UserId) (SELECT DISTINCT UserId FROM @activity);
--			SET @user_count = (SELECT COUNT(*) FROM @user_ids);
--			DECLARE @user_iterator INT = 1;
--			WHILE(@user_iterator <= @user_count)
--			BEGIN
--				DELETE FROM @user_activity;
--				DECLARE @user_activity_duration FLOAT = 0;
--				DECLARE @current_user_id INT = NULL;
--				SET @current_user_id = (SELECT UserId FROM @user_ids ORDER BY CURRENT_TIMESTAMP OFFSET (@user_iterator - 1) ROWS FETCH NEXT 1 ROWS ONLY);
--				INSERT INTO @user_activity (UserId, ActivityDate) (SELECT UserId, ActivityDate FROM @activity WHERE UserId = @current_user_id) ORDER BY CURRENT_TIMESTAMP DESC;
--				DECLARE @user_activity_count INT = NULL;
--				SET @user_activity_count = (SELECT COUNT(*) FROM @user_activity);
--				DECLARE @user_activity_iterator INT = 1;
--				WHILE(@user_activity_iterator < @user_activity_count)
--				BEGIN
--					DECLARE @current_activity_date DATETIME2 = NULL;
--					SET @current_activity_date = (SELECT ActivityDate FROM @user_activity ORDER BY CURRENT_TIMESTAMP OFFSET (@user_activity_iterator - 1) ROWS FETCH NEXT 1 ROWS ONLY);
--					IF(@user_activity_iterator = @user_activity_count - 1)
--					BEGIN
--						SET @user_activity_duration = @user_activity_duration + 300;
--					END
--					ELSE
--					BEGIN
--						DECLARE @current_plus_one_activity_date DATETIME2 = NULL;
--						SET @current_plus_one_activity_date = (SELECT ActivityDate FROM @user_activity ORDER BY CURRENT_TIMESTAMP OFFSET (@user_activity_iterator) ROWS FETCH NEXT 1 ROWS ONLY);
--						DECLARE @difference FLOAT = 0;
--						SET @difference = (SELECT DATEDIFF(MILLISECOND, @current_activity_date, @current_plus_one_activity_date));
--						IF(@difference < 300)
--						BEGIN
--							SET @user_activity_duration = @user_activity_duration + @difference;
--						END
--					END
--					SET @user_activity_iterator = @user_activity_iterator + 1;
--				END
--				INSERT INTO @user_duration (Duration, UserId) SELECT @user_activity_duration, @current_user_id;
--				SET @user_iterator = @user_iterator + 1;
--			END
--			DECLARE @total_duration FLOAT = 0;
--			DECLARE @total_user_count INT = 0;
--			SET @total_user_count = (SELECT COUNT(*) FROM @user_duration);
--			SET @total_duration = (SELECT SUM(Duration) FROM @user_duration);
--			DECLARE @average FLOAT = 0;
--			SET @average = @total_duration / 60;
--			SET @average = @average / @total_user_count;
--			UPDATE #result_table SET Duration = @average WHERE Day = (SELECT Day FROM #result_table ORDER BY CURRENT_TIMESTAMP OFFSET(13 - 1) ROWS FETCH NEXT 1 ROWS ONLY);
--			--SET @counter = @counter - 1;
--			SET @counterd = @counterd - 1;
--		END
--		SELECT * FROM #result_table;
DROP TABLE IF EXISTS #result_table
GO
CREATE TABLE #result_table
		(
			Day DATETIME2,
			Duration FLOAT,
			Iterator INT
		)
		DECLARE @counter INT = 14;
		DECLARE @activity TABLE
		(
			UserId INT, 
			ActivityDate DATETIME2
		);
		DECLARE @user_ids TABLE
		(
			UserId INT
		)
		DECLARE @user_activity TABLE
		(
			UserId INT, 
			ActivityDate DATETIME2
		);
		DECLARE @user_duration TABLE
		(
			Duration FLOAT,
			UserId INT
		)
		WHILE(@counter > 0)
		BEGIN
			DELETE FROM @activity;
			DELETE FROM @user_ids;
			DELETE FROM @user_duration;
			DECLARE @iteration_day DATETIME2 = CAST(DATEADD(Day, (@counter * -1), GETDATE()) As date);;
			INSERT INTO #result_table (Day, Duration, Iterator) VALUES (@iteration_day, 0, @counter);
			DECLARE @user_count INT = 0;
			INSERT INTO @activity (UserId, ActivityDate) (SELECT UserId, ActivityDate
				FROM [Statistics] Where CAST(CreatedDt as date) = @iteration_day);
			INSERT INTO @user_ids (UserId) (SELECT DISTINCT UserId FROM @activity);
			SET @user_count = (SELECT COUNT(*) FROM @user_ids);
			DECLARE @user_iterator INT = 1;
			WHILE(@user_iterator <= @user_count)
			BEGIN
				DELETE FROM @user_activity;
				DECLARE @user_activity_duration FLOAT = 0;
				DECLARE @current_user_id INT = 0;
				SET @current_user_id = (SELECT UserId FROM @user_ids ORDER BY CURRENT_TIMESTAMP OFFSET (@user_iterator - 1) ROWS FETCH NEXT 1 ROWS ONLY);
				INSERT INTO @user_activity (UserId, ActivityDate) (SELECT UserId, ActivityDate FROM @activity WHERE UserId = @current_user_id) ORDER BY CURRENT_TIMESTAMP DESC;
				DECLARE @user_activity_count INT = 0;
				SET @user_activity_count = (SELECT COUNT(*) FROM @user_activity);
				DECLARE @user_activity_iterator INT = 1;
				WHILE(@user_activity_iterator < @user_activity_count)
				BEGIN
					DECLARE @current_activity_date DATETIME2 = (SELECT ActivityDate FROM @user_activity ORDER BY CURRENT_TIMESTAMP OFFSET (@user_activity_iterator - 1) ROWS FETCH NEXT 1 ROWS ONLY);
					IF(@user_activity_iterator = @user_activity_count - 1)
					BEGIN
						SET @user_activity_duration = @user_activity_duration + 300;
					END
					ELSE
					BEGIN
						DECLARE @current_plus_one_activity_date DATETIME2 = (SELECT ActivityDate FROM @user_activity ORDER BY CURRENT_TIMESTAMP OFFSET (@user_activity_iterator) ROWS FETCH NEXT 1 ROWS ONLY);
						DECLARE @difference FLOAT = 0;
						SET @difference = DATEDIFF(SECOND, @current_activity_date, @current_plus_one_activity_date);
						IF(@difference < 300)
						BEGIN
							SET @user_activity_duration = @user_activity_duration + @difference;
						END
					END
					SET @user_activity_iterator = @user_activity_iterator + 1;
				END
				INSERT INTO @user_duration (Duration, UserId) SELECT @user_activity_duration, @current_user_id;
				SET @user_iterator = @user_iterator + 1;
			END
			DECLARE @total_duration FLOAT = 0;
			DECLARE @total_user_count INT = 0;
			SET @total_user_count = (SELECT COUNT(*) FROM @user_duration);
			SET @total_duration = (SELECT SUM(Duration) FROM @user_duration);
			DECLARE @average FLOAT = 0;
			SET @average = @total_duration / 60;
			SET @average = @average / @total_user_count;
			IF(@average IS NULL)
			BEGIN
				SET @average = 0;
			END
			UPDATE #result_table SET Duration = @average WHERE Iterator = @counter;
			SET @counter = @counter - 1;
		END
		SELECT * FROM #result_table;


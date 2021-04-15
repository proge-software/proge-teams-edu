declare @id uniqueidentifier
set @id = '32440c56-b584-47d9-a175-f89a4c99a56f'

delete from CallSessionSegment where CallSessionId in (select Id from CallSession where CallRecordId = @id)
delete from CallSession where CallRecordId = @id
delete from CallUser where CallRecordId = @id
delete from CallRecord where Id = @id

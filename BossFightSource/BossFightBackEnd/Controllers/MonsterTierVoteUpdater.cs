using BossFight;
using BossFight.BossFightEnums;
using BossFight.Extentions;
using BossFight.Models;
using Microsoft.AspNetCore.ResponseCompression;

public static class MonsterTierVoteUpdater
{
    public static void UpdatePlayersMonsterTierVote(int pPlayerId, int pMonsterInstanceId, int pVote)
    {
        var newVote = (MonsterTierVoteChoice)pVote;
        var existingVote = new MonsterTierVote{PlayerId = pPlayerId, MonsterInstanceId = pMonsterInstanceId}.FindOne(null);
        if (existingVote != null)
        {
            if (existingVote.Vote != newVote)
            {
                existingVote.Vote = newVote;
                existingVote.Persist();
            }
        }
        else
        {
            var monsterTierVote = new MonsterTierVote{PlayerId = pPlayerId, MonsterInstanceId = pMonsterInstanceId, Vote = newVote };
            monsterTierVote.Persist();
        }
    }

    public static MonsterTierVote PlayersCurrentMonsterTierVote(int pPlayerId)
    {
        var monsterTierVote = new MonsterTierVote();
        var currentMonster = new MonsterInstance{ Active = true }.FindOne();
        if (currentMonster != null)
        {
            monsterTierVote = new MonsterTierVote{ PlayerId = pPlayerId, MonsterInstanceId = currentMonster.MonsterInstanceId }.FindOne();
        }

        return monsterTierVote;
    }

    public record MonsterTierVotesTotal(int UpVotes, int DownVotes);
    
    public static MonsterTierVotesTotal CountMonsterTierVotesTotalForActiveMonster()
    {
        var sql = @"SELECT count(IF (mtv.Vote = 1, 1, NULL)) as VoteUp, count(IF (mtv.Vote = -1, 1, NULL)) as VoteDown
FROM MonsterTierVote mtv 
JOIN MonsterInstance mi 
	ON mi.MonsterInstanceId = mtv.MonsterInstanceId 
	AND mi.Active = 1";

        using var connection = GlobalConnection.GetNewOpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText= sql;
        var reader = cmd.ExecuteReader();
        reader.Read();
        var upVotes = reader.GetInt("VoteUp");
        var downVotes = reader.GetInt("VoteDown");
        reader.Close();
        connection.Close();
        
        return new MonsterTierVotesTotal(upVotes, downVotes);
    }
}

using BossFight.BossFightEnums;
using BossFight.Models;

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
}
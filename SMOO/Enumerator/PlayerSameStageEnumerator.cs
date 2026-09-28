using SMOO.Client;

namespace SMOO.Enumerator;

/// <summary>
/// An enumerator of connected players in the same stage as a target player. The enumerator does not include
/// the target player in the iteration
/// </summary>
internal ref struct PlayerSameStageEnumerator : IPlayerEnumerator<PlayerSameStageEnumerator>
{
    private PlayerInStateEnumerator _connectedEnumerator;
    private readonly Player _targetPlayer;
    public Player Current => _connectedEnumerator.Current;
    public PlayerSameStageEnumerator GetEnumerator() => this;

    public PlayerSameStageEnumerator(ReadOnlySpan<Player> players, Player targetStagePlayer)
    {
        _connectedEnumerator = new PlayerInStateEnumerator(players, PlayerState.Connected);
        _targetPlayer = targetStagePlayer;
    }

    public bool MoveNext()
    {
        while (_connectedEnumerator.MoveNext())
        {
            if (_connectedEnumerator.Current == _targetPlayer)
            {
                continue;
            }

            if (_connectedEnumerator.Current.WorldInfo.CurrentStage == _targetPlayer.WorldInfo.CurrentStage)
            {
                return true;
            }
        }

        return false;
    }

    public readonly void Dispose()
    {

    }
}

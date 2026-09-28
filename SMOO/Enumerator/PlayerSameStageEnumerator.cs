using SMOO.Client;

namespace SMOO.Enumerator;

/// <summary>
/// An enumerator of players that are not disconnecting, in the same stage as a target player. The enumerator does not include
/// the target player in the iteration
/// </summary>
internal ref struct PlayerSameStageEnumerator : IPlayerEnumerator<PlayerSameStageEnumerator>
{
    private PlayerNotInStateEnumerator _playerEnumerator;
    private readonly Player _targetPlayer;
    public Player Current => _playerEnumerator.Current;
    public PlayerSameStageEnumerator GetEnumerator() => this;

    public PlayerSameStageEnumerator(ReadOnlySpan<Player> players, Player targetStagePlayer)
    {
        _playerEnumerator = new PlayerNotInStateEnumerator(players, PlayerState.Disconnecting);
        _targetPlayer = targetStagePlayer;
    }

    public bool MoveNext()
    {
        while (_playerEnumerator.MoveNext())
        {
            if (_playerEnumerator.Current == _targetPlayer)
            {
                continue;
            }

            if (_playerEnumerator.Current.WorldInfo.CurrentStage == _targetPlayer.WorldInfo.CurrentStage)
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

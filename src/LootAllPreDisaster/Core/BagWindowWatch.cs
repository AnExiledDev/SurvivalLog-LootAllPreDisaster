using System;

namespace LootAllPreDisaster.Core;

/// <summary>
/// Closes the bag window a search opens for a container the run takes from. The window can appear a moment after the
/// run has already emptied the container, and it then lists items that are no longer there, so it is closed once it
/// shows rather than when the container is emptied. A window for a container the run did not take from is the
/// player's to use and is left open.
/// </summary>
public sealed class BagWindowWatch
{
    /// <summary>How long a window the game opened may take to show before the watch stops waiting for it.</summary>
    public const float WaitSeconds = 2f;

    private long? _containerId;
    private float _openedAt;

    public void Opened(long containerId, float now)
    {
        _containerId = containerId;
        _openedAt = now;
    }

    /// <summary>
    /// Whether to close the bag window now. It waits while the window is not shown yet, and while the run is still
    /// about to empty the container.
    /// </summary>
    public bool ShouldClose(Func<bool> isWindowShown, Func<long, bool> wasTakenFrom, Func<long, bool> isDueForTake, float now)
    {
        if (_containerId is not { } containerId) return false;

        if (now - _openedAt > WaitSeconds)
        {
            _containerId = null;

            return false;
        }

        var isTaken = wasTakenFrom(containerId);

        if (!isWindowShown() || (!isTaken && isDueForTake(containerId))) return false;

        _containerId = null;

        return isTaken;
    }
}

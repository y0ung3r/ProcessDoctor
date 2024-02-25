using System.ComponentModel.DataAnnotations;

namespace ProcessDoctor.Backend.Linux.Proc.Enums;

public enum ProcessState
{
    /// <summary>
    /// Running
    /// </summary>
    [Display(Name = "R")]
    Running,

    /// <summary>
    /// Sleeping
    /// </summary>
    [Display(Name = "S")]
    Sleeping,

    /// <summary>
    /// Sleeping in an uninterruptible wait
    /// </summary>
    [Display(Name = "D")]
    UninterruptibleWait,

    /// <summary>
    /// Zombie
    /// </summary>
    [Display(Name = "Z")]
    Zombie,

    /// <summary>
    /// Traced or stopped
    /// </summary>
    [Display(Name = "T")]
    TracedOrStopped
}

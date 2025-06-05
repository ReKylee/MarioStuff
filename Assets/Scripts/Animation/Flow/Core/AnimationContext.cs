using System.Collections.Generic;
using Animation.Flow.Interfaces;

namespace Animation.Flow.Core
{
    /// <summary>
    ///     Context for animation flow execution
    ///     Contains parameters and the animator interface
    /// </summary>
    public class AnimationContext
    {
        // The animator interface used to play animations
        private readonly IAnimator _animator;

        // Dictionary to store parameters of different types
        private readonly Dictionary<string, bool> _boolParams = new();
        private readonly Dictionary<string, int> _intParams = new();
        private readonly Dictionary<string, float> _floatParams = new();
        private readonly Dictionary<string, string> _stringParams = new();

        /// <summary>
        ///     Creates a new animation context with the specified animator
        /// </summary>
        /// <param name="animator">The animator to use for playing animations</param>
        public AnimationContext(IAnimator animator)
        {
            _animator = animator;
        }

        /// <summary>
        ///     Gets the animator interface
        /// </summary>
        public IAnimator Animator => _animator;

        #region Parameter Access

        /// <summary>
        ///     Sets a boolean parameter
        /// </summary>
        public void SetBool(string name, bool value)
        {
            _boolParams[name] = value;
        }

        /// <summary>
        ///     Gets a boolean parameter
        /// </summary>
        public bool GetBool(string name, bool defaultValue = false)
        {
            return _boolParams.TryGetValue(name, out bool value) ? value : defaultValue;
        }

        /// <summary>
        ///     Sets an integer parameter
        /// </summary>
        public void SetInt(string name, int value)
        {
            _intParams[name] = value;
        }

        /// <summary>
        ///     Gets an integer parameter
        /// </summary>
        public int GetInt(string name, int defaultValue = 0)
        {
            return _intParams.TryGetValue(name, out int value) ? value : defaultValue;
        }

        /// <summary>
        ///     Sets a float parameter
        /// </summary>
        public void SetFloat(string name, float value)
        {
            _floatParams[name] = value;
        }

        /// <summary>
        ///     Gets a float parameter
        /// </summary>
        public float GetFloat(string name, float defaultValue = 0f)
        {
            return _floatParams.TryGetValue(name, out float value) ? value : defaultValue;
        }

        /// <summary>
        ///     Sets a string parameter
        /// </summary>
        public void SetString(string name, string value)
        {
            _stringParams[name] = value;
        }

        /// <summary>
        ///     Gets a string parameter
        /// </summary>
        public string GetString(string name, string defaultValue = "")
        {
            return _stringParams.TryGetValue(name, out string value) ? value : defaultValue;
        }

        /// <summary>
        ///     Clears all parameters
        /// </summary>
        public void ClearParameters()
        {
            _boolParams.Clear();
            _intParams.Clear();
            _floatParams.Clear();
            _stringParams.Clear();
        }

        #endregion
    }
}

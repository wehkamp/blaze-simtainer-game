using TMPro;
using UnityEngine;

namespace Assets.Scripts.Components
{
	/// <summary>
	/// FPS Counter. Use fps 1 to enable in debug console and 0 to disable.
	/// Default disabled.
	/// </summary>
	public class FpsCounter : MonoBehaviour
	{
		private TMP_Text _text;
		[SerializeField] private float _hudRefreshRate = 1f;

		private float _timer;

		// Start is called before the first frame update
		void Start()
		{
			_text = GetComponent<TMP_Text>();
		}

		// Update is called once per frame
		void Update()
		{
			if (Time.unscaledTime > _timer)
			{
				int fps = (int)(1f / Time.unscaledDeltaTime);
				_text.text = fps + " FPS";
				_timer = Time.unscaledTime + _hudRefreshRate;
			}
		}
	}
}

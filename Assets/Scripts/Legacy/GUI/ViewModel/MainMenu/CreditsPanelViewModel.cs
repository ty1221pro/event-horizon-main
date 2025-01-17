using UnityEngine;
using UnityEngine.UI;

namespace ViewModel
{
	public class CreditsPanelViewModel : MonoBehaviour
	{
		[SerializeField] public RectTransform Panel;
		[SerializeField] public RectTransform Content;
		[SerializeField] public float TimeInSeconds = 120f;

		[SerializeField] private GameObject _gameTitle;
		[SerializeField] private GameObject _gameAlternativeTitle;

		private void Start()
		{
			if (!_startTime.HasValue)
				_startTime = Time.time;

			_gameTitle.SetActive(!AppConfig.alternativeTitle);
			_gameAlternativeTitle.SetActive(AppConfig.alternativeTitle);
		}

		private void Update()
		{
			var deltaTime = (Time.time - _startTime.Value) / TimeInSeconds; 
			deltaTime -= Mathf.Floor(deltaTime);
			var panelHeight = Panel.rect.height;
			var contentHeight = Content.sizeDelta.y;
			Content.anchoredPosition = new Vector2(0,(contentHeight+2*panelHeight)*deltaTime - panelHeight);
		}

		private static float? _startTime;
	}
}

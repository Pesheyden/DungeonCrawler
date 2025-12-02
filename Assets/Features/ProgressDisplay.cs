using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ProgressDisplay : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] _progressProviders;

    private IProgressProvider[] ProgressProvidersCasted => 
        _progressProviders.OfType<IProgressProvider>().ToArray();

    [SerializeField] private Slider _sliderBar;
    [SerializeField] private TextMeshProUGUI _text;

    private float _partMaxValue;

    private void Start()
    {
        _partMaxValue = 1f / ProgressProvidersCasted.Length;
    }

    private void Update()
    {
        for (var index = 0; index < ProgressProvidersCasted.Length; index++)
        {
            var progressProvider = ProgressProvidersCasted[index];
            Debug.Log(_progressProviders[index].name + ": " + progressProvider.GetProgress());
            if (progressProvider.GetProgress() >= 1)
                continue;
            _sliderBar.value = _partMaxValue * index;
            _sliderBar.value += _partMaxValue * progressProvider.GetProgress();
            _text.text = progressProvider.GetProgressTitle();
            break;
        }
    }
}

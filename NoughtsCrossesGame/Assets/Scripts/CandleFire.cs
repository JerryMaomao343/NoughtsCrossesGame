using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleFire : MonoBehaviour
{

    private Light _candleLight;

    public float minIntensity = 0.8f;
    public float maxIntensity = 1.2f;
    
    public float flickerSpeed = 5f;
    
    private float _noiseOffset;

    void Start()
    {
        if (_candleLight == null)
            _candleLight = GetComponent<Light>();
        
        _noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, _noiseOffset);

        _candleLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}

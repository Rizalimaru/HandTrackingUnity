using UnityEngine;

public class BackgroundKimia : MonoBehaviour
{
    void Start()
    {
        // Tambahkan komponen particle system otomatis
        var ps = gameObject.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.2f, 1f, 0.8f, 0.5f),   // warna hijau-biru transparan
            new Color(0.6f, 0.2f, 1f, 0.5f));  // warna ungu transparan
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
        main.startLifetime = 2f;
        main.startSpeed = 0.2f;
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Emission
        var emission = ps.emission;
        emission.rateOverTime = 20f;

        // Bentuk lingkaran biar kayak aura kimia
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;

        // Tambahin efek glow
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.cyan, 0.0f),
                new GradientColorKey(Color.magenta, 1.0f)},
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.5f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)}
        );
        colorOverLifetime.color = grad;

        // Tambahin sedikit movement random
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.5f;
    }
}

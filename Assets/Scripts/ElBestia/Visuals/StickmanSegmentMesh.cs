using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElBestia.Visuals
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class StickmanSegmentMesh : MonoBehaviour
    {
        private const int MinSections = 2;
        private const int MaxSections = 16;
        private const int MinRadialSegments = 6;
        private const int MaxRadialSegments = 32;
        private const int MinRingsPerSection = 2;
        private const int MaxRingsPerSection = 8;

        [SerializeField] private Transform startJoint;
        [SerializeField] private Transform endJoint;
        [SerializeField, Range(MinSections, MaxSections)] private int sectionCount = 3;
        [SerializeField, Range(MinRadialSegments, MaxRadialSegments)] private int radialSegments = 16;
        [SerializeField, Range(MinRingsPerSection, MaxRingsPerSection)] private int ringsPerSection = 3;
        [SerializeField, Range(0.05f, 1f)] private float capHeightFactor = 0.35f;
        [SerializeField, Range(0f, 1f)] private float endInsetFactor = 0.25f;
        [SerializeField, Range(0.001f, 1f)] private float minRadius = 0.02f;
        [SerializeField] private bool useCustomStartMinRadius;
        [SerializeField, Range(0.001f, 1f)] private float customStartMinRadius = 0.02f;
        [SerializeField, Range(0.005f, 1f)] private float maxRadius = 0.35f;
        [SerializeField, Range(0.001f, 1f)] private float maxRadiusDelta = 0.3f;
        [SerializeField] private bool smoothRadii = true;
        [SerializeField] private bool rebuildContinuously = true;
        [SerializeField] private List<float> sectionRadii = new List<float> { 0.08f, 0.12f, 0.08f };

        [NonSerialized] private Mesh generatedMesh;
        [NonSerialized] private MeshFilter cachedMeshFilter;
        [NonSerialized] private Vector3 lastStartPosition;
        [NonSerialized] private Vector3 lastEndPosition;

        public int SectionCount => sectionCount;
        public float MaxRadius => maxRadius;
        public float GetSectionMinRadius(int index) => index == 0 && useCustomStartMinRadius ? customStartMinRadius : minRadius;

        public float GetSectionRadius(int index)
        {
            EnsureRadiusCount();
            return sectionRadii[Mathf.Clamp(index, 0, sectionRadii.Count - 1)];
        }

        public void SetSectionCount(int count)
        {
            sectionCount = Mathf.Clamp(count, MinSections, MaxSections);
            EnsureRadiusCount();
            Rebuild();
        }

        public void SetSectionRadius(int index, float radius)
        {
            EnsureRadiusCount();
            if (index < 0 || index >= sectionRadii.Count)
            {
                return;
            }

            sectionRadii[index] = Mathf.Clamp(radius, GetSectionMinRadius(index), maxRadius);
            ApplyRadiusConstraints(index);
            Rebuild();
        }

        public void Rebuild()
        {
            EnsureMesh();
            EnsureRadiusCount();

            if (startJoint == null || endJoint == null)
            {
                generatedMesh.Clear();
                return;
            }

            Vector3 start = transform.InverseTransformPoint(startJoint.position);
            Vector3 end = transform.InverseTransformPoint(endJoint.position);
            Vector3 axis = end - start;
            float length = axis.magnitude;

            if (length <= 0.0001f)
            {
                generatedMesh.Clear();
                return;
            }

            Vector3 forward = axis / length;
            BuildBasis(forward, out Vector3 right, out Vector3 up);
            BuildMesh(start, axis, right, up);

            lastStartPosition = startJoint.position;
            lastEndPosition = endJoint.position;
        }

        private void OnEnable()
        {
            EnsureMesh();
            Rebuild();
        }

        private void OnDisable()
        {
            if (cachedMeshFilter != null && cachedMeshFilter.sharedMesh == generatedMesh)
            {
                cachedMeshFilter.sharedMesh = null;
            }

            if (generatedMesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedMesh);
            }
            else
            {
                DestroyImmediate(generatedMesh);
            }

            generatedMesh = null;
        }

        private void OnValidate()
        {
            sectionCount = Mathf.Clamp(sectionCount, MinSections, MaxSections);
            radialSegments = Mathf.Clamp(radialSegments, MinRadialSegments, MaxRadialSegments);
            ringsPerSection = Mathf.Clamp(ringsPerSection, MinRingsPerSection, MaxRingsPerSection);
            capHeightFactor = Mathf.Clamp(capHeightFactor, 0.05f, 1f);
            endInsetFactor = Mathf.Clamp01(endInsetFactor);
            minRadius = Mathf.Max(0.001f, minRadius);
            customStartMinRadius = Mathf.Max(0.001f, customStartMinRadius);
            maxRadius = Mathf.Max(minRadius, maxRadius);
            maxRadiusDelta = Mathf.Max(0.001f, maxRadiusDelta);
            EnsureRadiusCount();
            Rebuild();
        }

        private void Update()
        {
            if (!rebuildContinuously)
            {
                return;
            }

            if (startJoint == null || endJoint == null)
            {
                return;
            }

            if (startJoint.position != lastStartPosition || endJoint.position != lastEndPosition)
            {
                Rebuild();
            }
        }

        private void EnsureMesh()
        {
            if (cachedMeshFilter == null)
            {
                cachedMeshFilter = GetComponent<MeshFilter>();
            }

            if (generatedMesh == null)
            {
                generatedMesh = new Mesh
                {
                    name = $"{nameof(StickmanSegmentMesh)}_{gameObject.name}"
                };
                generatedMesh.MarkDynamic();
            }

            if (cachedMeshFilter.sharedMesh != generatedMesh)
            {
                cachedMeshFilter.sharedMesh = generatedMesh;
            }
        }

        private void EnsureRadiusCount()
        {
            if (sectionRadii == null)
            {
                sectionRadii = new List<float>();
            }

            while (sectionRadii.Count < sectionCount)
            {
                float value = sectionRadii.Count > 0 ? sectionRadii[sectionRadii.Count - 1] : Mathf.Max(minRadius, 0.08f);
                sectionRadii.Add(Mathf.Clamp(value, minRadius, maxRadius));
            }

            while (sectionRadii.Count > sectionCount)
            {
                sectionRadii.RemoveAt(sectionRadii.Count - 1);
            }

            for (int i = 0; i < sectionRadii.Count; i++)
            {
                sectionRadii[i] = Mathf.Clamp(sectionRadii[i], GetSectionMinRadius(i), maxRadius);
            }

            ApplyRadiusConstraints(-1);
        }

        private void ApplyRadiusConstraints(int editedIndex)
        {
            if (sectionRadii == null || sectionRadii.Count == 0)
            {
                return;
            }

            if (editedIndex >= 0)
            {
                for (int i = editedIndex - 1; i >= 0; i--)
                {
                    sectionRadii[i] = Mathf.Clamp(sectionRadii[i], sectionRadii[i + 1] - maxRadiusDelta, sectionRadii[i + 1] + maxRadiusDelta);
                    sectionRadii[i] = Mathf.Clamp(sectionRadii[i], GetSectionMinRadius(i), maxRadius);
                }

                for (int i = editedIndex + 1; i < sectionRadii.Count; i++)
                {
                    sectionRadii[i] = Mathf.Clamp(sectionRadii[i], sectionRadii[i - 1] - maxRadiusDelta, sectionRadii[i - 1] + maxRadiusDelta);
                    sectionRadii[i] = Mathf.Clamp(sectionRadii[i], GetSectionMinRadius(i), maxRadius);
                }

                return;
            }

            for (int i = 1; i < sectionRadii.Count; i++)
            {
                sectionRadii[i] = Mathf.Clamp(sectionRadii[i], sectionRadii[i - 1] - maxRadiusDelta, sectionRadii[i - 1] + maxRadiusDelta);
                sectionRadii[i] = Mathf.Clamp(sectionRadii[i], GetSectionMinRadius(i), maxRadius);
            }
        }

        private void BuildMesh(Vector3 start, Vector3 axis, Vector3 right, Vector3 up)
        {
            float startRadius = EvaluateRadius(0f);
            float endRadius = EvaluateRadius(1f);
            Vector3 forward = axis.normalized;
            Vector3 end = start + axis;
            float length = axis.magnitude;
            float startInset = Mathf.Min(startRadius * endInsetFactor, length * 0.49f);
            float endInset = Mathf.Min(endRadius * endInsetFactor, length * 0.49f);
            float insetSum = startInset + endInset;
            if (insetSum > length * 0.98f)
            {
                float scale = (length * 0.98f) / insetSum;
                startInset *= scale;
                endInset *= scale;
            }

            Vector3 bodyStart = start + forward * startInset;
            Vector3 bodyEnd = end - forward * endInset;
            Vector3 bodyAxis = bodyEnd - bodyStart;

            int bodyRingCount = (sectionCount - 1) * ringsPerSection + 1;
            int capSegments = Mathf.Max(2, ringsPerSection);
            int capInnerRingCount = capSegments - 1;
            int bodyVertexCount = bodyRingCount * radialSegments;
            int startCapStartIndex = bodyVertexCount;
            int endCapStartIndex = startCapStartIndex + capInnerRingCount * radialSegments;
            int startPoleIndex = endCapStartIndex + capInnerRingCount * radialSegments;
            int endPoleIndex = startPoleIndex + 1;
            int vertexCount = endPoleIndex + 1;
            int bodyTriangleCount = (bodyRingCount - 1) * radialSegments * 6;
            int capTriangleCount = (capInnerRingCount * radialSegments * 6) + (radialSegments * 3);
            var vertices = new Vector3[vertexCount];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[bodyTriangleCount + capTriangleCount * 2];

            for (int ring = 0; ring < bodyRingCount; ring++)
            {
                float t = bodyRingCount == 1 ? 0f : ring / (float)(bodyRingCount - 1);
                float radius = EvaluateRadius(t);
                Vector3 center = bodyStart + bodyAxis * t;

                for (int segment = 0; segment < radialSegments; segment++)
                {
                    float angle = (segment / (float)radialSegments) * Mathf.PI * 2f;
                    Vector3 normal = Mathf.Cos(angle) * right + Mathf.Sin(angle) * up;
                    int vertexIndex = ring * radialSegments + segment;
                    vertices[vertexIndex] = center + normal * radius;
                    uvs[vertexIndex] = new Vector2(segment / (float)radialSegments, t);
                }
            }

            for (int capRing = 0; capRing < capInnerRingCount; capRing++)
            {
                float capT = (capRing + 1) / (float)capSegments;
                int startRingIndex = startCapStartIndex + capRing * radialSegments;
                int endRingIndex = endCapStartIndex + capRing * radialSegments;

                WriteSphericalCapRing(vertices, uvs, startRingIndex, bodyStart, start, right, up, startRadius, capT, 0f);
                WriteSphericalCapRing(vertices, uvs, endRingIndex, bodyEnd, end, right, up, endRadius, capT, 1f);
            }

            vertices[startPoleIndex] = start;
            vertices[endPoleIndex] = end;
            uvs[startPoleIndex] = new Vector2(0.5f, 0f);
            uvs[endPoleIndex] = new Vector2(0.5f, 1f);

            int triangleIndex = 0;
            for (int ring = 0; ring < bodyRingCount - 1; ring++)
            {
                int nextRing = ring + 1;
                for (int segment = 0; segment < radialSegments; segment++)
                {
                    int nextSegment = (segment + 1) % radialSegments;
                    int a = ring * radialSegments + segment;
                    int b = ring * radialSegments + nextSegment;
                    int c = nextRing * radialSegments + segment;
                    int d = nextRing * radialSegments + nextSegment;

                    triangles[triangleIndex++] = a;
                    triangles[triangleIndex++] = b;
                    triangles[triangleIndex++] = c;
                    triangles[triangleIndex++] = b;
                    triangles[triangleIndex++] = d;
                    triangles[triangleIndex++] = c;
                }
            }

            int lastBodyRingStart = (bodyRingCount - 1) * radialSegments;
            int startPreviousRing = 0;
            int endPreviousRing = lastBodyRingStart;
            for (int capRing = 0; capRing < capInnerRingCount; capRing++)
            {
                int startCurrentRing = startCapStartIndex + capRing * radialSegments;
                int endCurrentRing = endCapStartIndex + capRing * radialSegments;

                for (int segment = 0; segment < radialSegments; segment++)
                {
                    int nextSegment = (segment + 1) % radialSegments;

                    int startA = startPreviousRing + segment;
                    int startB = startPreviousRing + nextSegment;
                    int startC = startCurrentRing + segment;
                    int startD = startCurrentRing + nextSegment;
                    triangles[triangleIndex++] = startA;
                    triangles[triangleIndex++] = startC;
                    triangles[triangleIndex++] = startB;
                    triangles[triangleIndex++] = startB;
                    triangles[triangleIndex++] = startC;
                    triangles[triangleIndex++] = startD;

                    int endA = endPreviousRing + segment;
                    int endB = endPreviousRing + nextSegment;
                    int endC = endCurrentRing + segment;
                    int endD = endCurrentRing + nextSegment;
                    triangles[triangleIndex++] = endA;
                    triangles[triangleIndex++] = endB;
                    triangles[triangleIndex++] = endC;
                    triangles[triangleIndex++] = endB;
                    triangles[triangleIndex++] = endD;
                    triangles[triangleIndex++] = endC;
                }

                startPreviousRing = startCurrentRing;
                endPreviousRing = endCurrentRing;
            }

            for (int segment = 0; segment < radialSegments; segment++)
            {
                int nextSegment = (segment + 1) % radialSegments;

                triangles[triangleIndex++] = startPoleIndex;
                triangles[triangleIndex++] = startPreviousRing + nextSegment;
                triangles[triangleIndex++] = startPreviousRing + segment;

                triangles[triangleIndex++] = endPoleIndex;
                triangles[triangleIndex++] = endPreviousRing + segment;
                triangles[triangleIndex++] = endPreviousRing + nextSegment;
            }

            generatedMesh.Clear();
            generatedMesh.vertices = vertices;
            generatedMesh.uv = uvs;
            generatedMesh.triangles = triangles;
            generatedMesh.RecalculateNormals();
            generatedMesh.RecalculateBounds();
        }

        private void WriteRing(Vector3[] vertices, Vector2[] uvs, int ringStart, Vector3 center, Vector3 right, Vector3 up, float radius, float v)
        {
            for (int segment = 0; segment < radialSegments; segment++)
            {
                float angle = (segment / (float)radialSegments) * Mathf.PI * 2f;
                Vector3 normal = Mathf.Cos(angle) * right + Mathf.Sin(angle) * up;
                int vertexIndex = ringStart + segment;
                vertices[vertexIndex] = center + normal * radius;
                uvs[vertexIndex] = new Vector2(segment / (float)radialSegments, v);
            }
        }

        private void WriteSphericalCapRing(Vector3[] vertices, Vector2[] uvs, int ringStart, Vector3 seamCenter, Vector3 pole, Vector3 right, Vector3 up, float seamRadius, float t, float v)
        {
            Vector3 toPole = pole - seamCenter;
            float capHeight = toPole.magnitude;
            if (capHeight <= 0.0001f)
            {
                float fallbackRadius = Mathf.Lerp(seamRadius, 0f, Mathf.Clamp01(t));
                WriteRing(vertices, uvs, ringStart, seamCenter, right, up, fallbackRadius, v);
                return;
            }

            Vector3 capAxis = toPole / capHeight;
            float shapedT = Mathf.Pow(Mathf.Clamp01(t), Mathf.Lerp(1.75f, 0.65f, capHeightFactor));
            float centerDistance = capHeight * shapedT;
            float ringRadius = seamRadius * Mathf.Sqrt(Mathf.Max(0f, 1f - shapedT * shapedT));
            Vector3 center = seamCenter + capAxis * centerDistance;
            WriteRing(vertices, uvs, ringStart, center, right, up, ringRadius, v);
        }

        private float EvaluateRadius(float t)
        {
            if (sectionRadii.Count == 1)
            {
                return sectionRadii[0];
            }

            float scaled = Mathf.Clamp01(t) * (sectionRadii.Count - 1);
            int left = Mathf.FloorToInt(scaled);
            int right = Mathf.Min(left + 1, sectionRadii.Count - 1);
            float localT = scaled - left;

            if (smoothRadii)
            {
                localT = localT * localT * (3f - 2f * localT);
            }

            return Mathf.Lerp(sectionRadii[left], sectionRadii[right], localT);
        }

        private static void BuildBasis(Vector3 forward, out Vector3 right, out Vector3 up)
        {
            Vector3 reference = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.95f ? Vector3.right : Vector3.up;
            right = Vector3.Cross(reference, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;
        }
    }
}

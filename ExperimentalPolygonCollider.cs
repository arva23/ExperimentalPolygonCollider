using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace ExperimentalPolygonCollider
{

    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteInEditMode]
    public class ExperimentalPolygonCollider : MonoBehaviour
    {


        // CONFIGURATION PANEL PARAMETERS
        
        [Tooltip("Mask detected geometry in order to debug collider computation.")]
        public bool debugCollider = false;
        [SerializeField]
        [HideInInspector]
        private bool recentDebugCollider;


        [Tooltip("Set alpha tolerance threshold in order to determine the discretized, not continuous " +
            "edges of the object.")]
        [Range(0.0f, 1.0f)]
        public double alphaThreshold = 0.5f;
        [SerializeField]
        [HideInInspector]
        private double recentAlphaThreshold;


        [Tooltip("Set erosion radius in order to erase small details of the object.")]
        [Range(1, 10)]
        public int erosionRadius = 5;
        [SerializeField]
        [HideInInspector]
        private int recentErosionRadius;

        [Tooltip("Number of evaluations of erosion.")]
        [Range(0, 10)]
        public int numOfErosionIterations = 3;
        [SerializeField]
        [HideInInspector]
        private int recentNumOfErosionIterations;


        [Tooltip("Set dilation radius in order to unite small detail of the object.")]
        [Range(1, 10)]
        public int dilationRadius = 5;
        [SerializeField]
        [HideInInspector]
        private int recentDilationRadius;

        [Tooltip("Number of evaluations of dilation.")]
        [Range(0, 10)]
        public int numOfDilationIterations = 3;
        [SerializeField]
        [HideInInspector]
        private int recentNumOfDilationIterations;


        // allowed maximum number of vertices to create collider with
        [Tooltip("Vertex number upper bound to restrict number of vertices in computed collider.")]
        [Range(3, 3000)]// TODO: need to be fixed
        public int numOfVertices = 100;
        [SerializeField]
        [HideInInspector]
        private int recentNumOfVertices;


        //[Tooltip("Apply changes during play mode as well.")]
        //public bool changesInPlayMode = false;
        //[SerializeField]
        //[HideInInspector]
        //private bool recentChangesInPlayMode;


        [SerializeField]
        [HideInInspector]
        private Sprite recentSprite;

        [SerializeField]
        [HideInInspector]
        private Rect recentRect = new Rect();

        [SerializeField]
        [HideInInspector]
        private Vector2 recentOffset = new Vector2(-99999.0f, -99999.0f);

        [SerializeField]
        [HideInInspector]
        private float recentPixelsPerUnit;

        [SerializeField]
        [HideInInspector]
        private bool recentFlipX;

        [SerializeField]
        [HideInInspector]
        private bool recentFlipY;


        private SpriteRenderer spriteRenderer;
        private PolygonCollider2D polygonCollider;
        UnityEngine.Color[] pixels;
        bool changedConfig = false;
        //bool updateRequested = false;

        private string errMsgs;

        List<Vector2> boundaryVertices;
        List<Vector2> boundaryVerticesYTrav;
        List<int> boundaryVerticesLineLengths;

        [HideInInspector]
        public Texture2D texture;
        [HideInInspector]
        public Rect rect;

        [HideInInspector]
        private int xOffset = 0;
        [HideInInspector]
        private int yOffset = 0;
        [HideInInspector]
        private int width = 0;
        [HideInInspector]
        private int height = 0;
        

        private void Awake() {


        }


        private void Start() {

            polygonCollider = GetComponent<PolygonCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            recentSprite = spriteRenderer.sprite;
            
        }


        private void UpdateConfigurationPanel() {

            // if sprite has changed, update its recent state
            ////if (spriteRenderer.sprite != recentSprite)
            ////    recentSprite = spriteRenderer.sprite;

            // update settings
            if (spriteRenderer != null) {

                if (recentOffset != spriteRenderer.sprite.pivot)
                    recentOffset = spriteRenderer.sprite.pivot;

                if (recentRect != spriteRenderer.sprite.rect)
                    recentRect = spriteRenderer.sprite.rect;
                
                if (recentPixelsPerUnit != spriteRenderer.sprite.pixelsPerUnit)
                    recentPixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;
                
                if (recentFlipX != spriteRenderer.flipX)
                    recentFlipX = spriteRenderer.flipX;
                
                if (recentFlipY != spriteRenderer.flipY)
                    recentFlipY = spriteRenderer.flipY;
            }

            if (debugCollider != recentDebugCollider)
                recentDebugCollider = debugCollider;


            if (alphaThreshold != recentAlphaThreshold)
                recentAlphaThreshold = alphaThreshold;

            if (erosionRadius != recentErosionRadius)
                recentErosionRadius = erosionRadius;

            if (numOfErosionIterations != recentNumOfErosionIterations)
                recentNumOfErosionIterations = numOfErosionIterations;

            if (dilationRadius != recentDilationRadius)
                recentDilationRadius = dilationRadius;

            if (numOfDilationIterations != recentNumOfDilationIterations)
                recentNumOfDilationIterations = numOfDilationIterations;

            if (numOfVertices != recentNumOfVertices)
                recentNumOfVertices = numOfVertices;

            /*if (changesInPlayMode != recentChangesInPlayMode) {

                recentChangesInPlayMode = changesInPlayMode;
                changesInPlayMode = true;
            }*/

        }

        // TODO
        // SEPARATE PANEL FUNCTION INTO DIFFERENT MEMBER FUNCTIONS

        //private void RecomputePolygon();

        private void Update()
        {
            // updating collider
            
            // TODO cache management
        }

        public void RecomputePolygon() {

            UpdateConfigurationPanel();

            spriteRenderer.sprite = recentSprite;
            Graphics.CopyTexture(spriteRenderer.sprite.texture, texture);

            xOffset = (int)spriteRenderer.sprite.rect.x;
            yOffset = (int)spriteRenderer.sprite.rect.y;
            width = (int)spriteRenderer.sprite.rect.width;
            height = (int)spriteRenderer.sprite.rect.height;
            //texture = new Texture2D((int)spriteRenderer.sprite.rect.width, (int)spriteRenderer.sprite.rect.height, spriteRenderer.sprite.t, false);
            
            // reconfiguring polygon
            
            PreprocessSprite();

            DetectSpriteRegions();

            if(debugCollider) MaskRegions();

            ComputeRawSections();

            /*
            RefineContour();

            ConstructMesh;

            if(!debugCollider) RenderCollider();            
            */
           }


        // POLYGON COMPUTATION SUBROUTINES

        private void PreprocessSprite() {

            // TODO
        }


        private bool DetectSpriteRegions() {

            // TODO
            return false;
        }


        private void MaskRegions() {

            // TODO
        }


        private void ComputeRawSections() {

            // TODO
        }


        private void ConstructTriplets() { 
        
            // TODO
        }


        private void RefineContour(int max_num_of_pts) {
        
            // TODO
        }

        private void ConstructMesh() { 
        
            // TODO
        }


        private void RenderCollider() {
        
            // drawing collider for debug
            // TODO
        }
    }
}
using UnityEngine;
using System.Collections;


public class UISlicedFilledSprite : UISprite {

    /// <summary>
    /// Final widget's color passed to the draw buffer.
    /// </summary>

    Color32 drawingColor
    {
        get
        {
            Color colF = color;
            colF.a = finalAlpha;
            if (premultipliedAlpha) colF = NGUITools.ApplyPMA(colF);

            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                colF.r = Mathf.Pow(colF.r, 2.2f);
                colF.g = Mathf.Pow(colF.g, 2.2f);
                colF.b = Mathf.Pow(colF.b, 2.2f);
            }
            return colF;
        }
    }
    public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
    {
        Texture tex = mainTexture;
        if (tex == null) return;

        if (mSprite == null) mSprite = atlas.GetSprite(spriteName);
        if (mSprite == null) return;

        Rect outer = new Rect(mSprite.x, mSprite.y, mSprite.width, mSprite.height);
        Rect inner = new Rect(mSprite.x + mSprite.borderLeft, mSprite.y + mSprite.borderTop,
            mSprite.width - mSprite.borderLeft - mSprite.borderRight,
            mSprite.height - mSprite.borderBottom - mSprite.borderTop);

        outer = NGUIMath.ConvertToTexCoords(outer, tex.width, tex.height);
        inner = NGUIMath.ConvertToTexCoords(inner, tex.width, tex.height);

        int offset = verts.size;
        SlicedFill(verts, uvs, cols, outer, inner);

        if (onPostFill != null)
            onPostFill(this, offset, verts, uvs, cols);
    }

    public override Vector4 drawingDimensions
    {
        get
        {
            Vector2 offset = pivotOffset;

            float x0 = -offset.x * mWidth;
            float y0 = -offset.y * mHeight;
            float x1 = x0 + mWidth;
            float y1 = y0 + mHeight;

            if (GetAtlasSprite() != null && mType != Type.Tiled)
            {
                int padLeft = mSprite.paddingLeft;
                int padBottom = mSprite.paddingBottom;
                int padRight = mSprite.paddingRight;
                int padTop = mSprite.paddingTop;

                int w = mSprite.width + padLeft + padRight;
                //int h = mSprite.height + padBottom + padTop;
                float px = 1f;
                float py = 1f;

                if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
                {
                    x0 += padRight * px;
                    x1 -= padLeft * px;
                }
                else
                {
                    x0 += padLeft * px;
                    x1 -= padRight * px;
                }

                if (mFlip == Flip.Vertically || mFlip == Flip.Both)
                {
                    y0 += padTop * py;
                    y1 -= padBottom * py;
                }
                else
                {
                    y0 += padBottom * py;
                    y1 -= padTop * py;
                }
            }

            Vector4 br = border * pixelSize;

            float fw = br.x + br.z;
            float fh = br.y + br.w;

            float vx = Mathf.Lerp(x0, x1 - fw, mDrawRegion.x);
            float vy = Mathf.Lerp(y0, y1 - fh, mDrawRegion.y);
            float vz = Mathf.Lerp(x0 + fw, x1, mDrawRegion.z);
            float vw = Mathf.Lerp(y0 + fh, y1, mDrawRegion.w);

            return new Vector4(vx, vy, vz, vw);
        }
    }

    void SlicedFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols, Rect mOuterUV, Rect mInnerUV)
    {
        Vector4 br = border * pixelSize;

        Color32 c = drawingColor;
        Vector4 v = drawingDimensions;

        float perc = fillAmount;

        float percentMaxLeft = br.x / mWidth;
        float percentMaxRight = br.z / mWidth;
        float percentMaxMiddle = 1F - (percentMaxLeft + percentMaxRight);

        float[] percentActual;

        if (mInvert == false)
        {
            percentActual = new float[]{
                Mathf.Clamp01(perc / percentMaxLeft),
                Mathf.Clamp01((perc - percentMaxLeft) / percentMaxMiddle),
                Mathf.Clamp01((perc - percentMaxLeft - percentMaxMiddle) / percentMaxRight),
                };

            mTempPos[0].x = v.x;
            mTempPos[1].x = v.x + br.x * percentActual[0];
            mTempPos[2].x = mTempPos[1].x + (v.z - v.x - br.x - br.z) * percentActual[1];
            mTempPos[3].x = v.z - br.z * (1f - percentActual[2]);

            mTempUVs[0].x = mOuterUV.xMin;
            mTempUVs[1].x = mOuterUV.xMin + (mInnerUV.xMin - mOuterUV.xMin) * percentActual[0];
            mTempUVs[2].x = mInnerUV.xMax;
            mTempUVs[3].x = mInnerUV.xMax + (mOuterUV.xMax - mInnerUV.xMax) * percentActual[2];
        }
        else
        {
            percentActual = new float[]{
                Mathf.Clamp01((perc - percentMaxRight - percentMaxMiddle) / percentMaxLeft),
                Mathf.Clamp01((perc - percentMaxRight) / percentMaxMiddle),
                Mathf.Clamp01(perc / percentMaxRight),
                };

            mTempPos[3].x = v.z;
            mTempPos[2].x = v.z - br.z * percentActual[2];
            mTempPos[1].x = mTempPos[2].x - (v.z - v.x - br.x - br.z) * percentActual[1];
            mTempPos[0].x = v.x + br.x * (1f - percentActual[0]);

            mTempUVs[3].x = mOuterUV.xMax;
            mTempUVs[2].x = mOuterUV.xMax - (mOuterUV.xMax - mInnerUV.xMax) * percentActual[2];
            mTempUVs[1].x = mInnerUV.xMin;
            mTempUVs[0].x = mInnerUV.xMin - (mInnerUV.xMin - mOuterUV.xMin) * percentActual[0];
        }

        mTempPos[0].y = v.y;
        mTempPos[1].y = mTempPos[0].y + br.y;
        mTempPos[3].y = v.w;
        mTempPos[2].y = mTempPos[3].y - br.w;

        mTempUVs[0].y = mOuterUV.yMin;
        mTempUVs[1].y = mInnerUV.yMin;
        mTempUVs[2].y = mInnerUV.yMax;
        mTempUVs[3].y = mOuterUV.yMax;

        for (int x = 0; x < 3; ++x)
        {
            if (percentActual[x] <= 0) continue;

            int x2 = x + 1;

            for (int y = 0; y < 3; ++y)
            {
                if (centerType == AdvancedType.Invisible && x == 1 && y == 1) continue;

                int y2 = y + 1;

                verts.Add(new Vector3(mTempPos[x].x, mTempPos[y].y));
                verts.Add(new Vector3(mTempPos[x].x, mTempPos[y2].y));
                verts.Add(new Vector3(mTempPos[x2].x, mTempPos[y2].y));
                verts.Add(new Vector3(mTempPos[x2].x, mTempPos[y].y));

                uvs.Add(new Vector2(mTempUVs[x].x, mTempUVs[y].y));
                uvs.Add(new Vector2(mTempUVs[x].x, mTempUVs[y2].y));
                uvs.Add(new Vector2(mTempUVs[x2].x, mTempUVs[y2].y));
                uvs.Add(new Vector2(mTempUVs[x2].x, mTempUVs[y].y));

                cols.Add(c);
                cols.Add(c);
                cols.Add(c);
                cols.Add(c);
            }
        }
    }
}

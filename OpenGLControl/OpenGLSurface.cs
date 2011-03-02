using System;
using OpenGLApp;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace OpenGLApp
{
	public class OpenGlSurface : OpenGLControl
	{
		//======= ����� ������ ������
		Color m_BkClr;		// ���� ���� ����
		public int[] m_LightParam = new int[11];	// ��������� ���������
		public uint m_FillMode;		// ����� ���������� ���������
		public bool m_bQuad;		// ���� ������������� GL_QUAD

		float m_AngleX;		// ���� �������� ������ ��� X
		float m_AngleY;		// ���� �������� ������ ��� Y
		float m_AngleView;	// ���� �����������
		float m_fRangeX;		// ������ ������� ����� X
		float m_fRangeY;		// ������ ������� ����� Y
		float m_fRangeZ;		// ������ ������� ����� Z
		float m_dx;			// ����� �������� ����� X
		float m_dy;			// ����� �������� ����� Y
		float m_xTrans;		// C������� ����� X
		float m_yTrans;		// C������� ����� Y
		float m_zTrans;		// C������� ����� Z
		bool m_bCaptured;	// ������� ������� ����

		Point m_pt;			// ������� ������� ����
		uint m_xSize;		// ������� ������ ���� ����� X
		uint m_zSize;		// ������� ������ ���� ����� Y
		Timer timer;

		//====== ������ ������ �����������
		CPoint3D[] m_cPoints;

		public OpenGlSurface()
		{
			Init(null);
		}

		public OpenGlSurface(float[][] points)
		{
			Init(points);
		}

		public void LoadMap(float[][] points)
		{
			InitGraphic(points);
			DrawScene();
			Invalidate();
		}


		private void Init(float[][] points)
		{
			//====== ��������� �������� �����������
			m_AngleX = 35.0f;
			m_AngleY = 20.0f;

			//====== ���� ������ ��� ������� ��������
			m_AngleView = 45.0f;

			//====== ��������� ���� ����
			m_BkClr = Color.FromArgb(0, 0, 96);

			// ��������� ����� ���������� ���������� ����� ��������
			m_FillMode = GL_LINE;

			//====== ���������� ������� �� ���������
			if (points == null)
				DefaultGraphic();
			else
			{
				InitGraphic(points);
			}

			//====== ��������� �������� ������������ ������ �����
			//====== ����� ����� �� ���������� ������ �������
			m_zTrans = -1.5f * m_fRangeX;
			m_xTrans = m_yTrans = 0.0f;

			//== ��������� �������� ������� �������� (��� ��������)
			m_dx = m_dy = 0.0f;

			//====== ���� �� ���������
			m_bCaptured = false;
			//====== ������ ������������������
			m_bQuad = true;

			//====== ��������� �������� ���������� ���������
			m_LightParam[0] = 50;	// X position
			m_LightParam[1] = 80;	// Y position
			m_LightParam[2] = 100;	// Z position
			m_LightParam[3] = 15;	// Ambient light
			m_LightParam[4] = 70;	// Diffuse light
			m_LightParam[5] = 100;	// Specular light
			m_LightParam[6] = 100;	// Ambient material
			m_LightParam[7] = 100;	// Diffuse material
			m_LightParam[8] = 40;	// Specular material
			m_LightParam[9] = 70;	// Shininess material
			m_LightParam[10] = 0;	// Emission material

			timer = new Timer();
			timer.Interval = 33;
			timer.Tick += new EventHandler(timer_Tick);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
			glMatrixMode(GL_MODELVIEW);
			glLoadIdentity();

			//======= ��������� ���������� ���������
			SetLight();

			//====== ������������ ������� �������������
			glTranslatef(m_xTrans, m_yTrans, m_zTrans);
			glRotatef(m_AngleX, 1, 0, 0);
			glRotatef(m_AngleY, 0, 1, 0);

			//====== ����� �������� ������ �� ������
			glCallList(1);

			//====== ������������ �������
			SwapBuffers(m_hDC);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			SetProjection();
		}

		protected override PIXELFORMATDESCRIPTOR SetPFD()
		{
			PIXELFORMATDESCRIPTOR pfd;
			pfd.nSize = (ushort)0;
			pfd.nVersion = 1;
			pfd.dwFlags = (uint)(PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER);
			pfd.iPixelType = (byte)PFD_TYPE_RGBA;
			pfd.cColorBits = 24;
			pfd.cRedBits = 24;
			pfd.cRedShift = 0;
			pfd.cGreenBits = 24;
			pfd.cGreenShift = 0;
			pfd.cBlueBits = 24;
			pfd.cBlueShift = 0;
			pfd.cAlphaBits = 24;
			pfd.cAlphaShift = 0;
			pfd.cAccumBits = 0;
			pfd.cAccumRedBits = 0;
			pfd.cAccumGreenBits = 0;
			pfd.cAccumBlueBits = 0;
			pfd.cAccumAlphaBits = 0;
			pfd.cDepthBits = 32;
			pfd.cStencilBits = 0;
			pfd.cAuxBuffers = 0;
			pfd.iLayerType = 0;
			pfd.bReserved = 0;
			pfd.dwLayerMask = 0;
			pfd.dwVisibleMask = 0;
			pfd.dwDamageMask = 0;
			return pfd;
		}
		protected void SetProjection()
		{
			//====== ���������� ������������ ����
			double dAspect = Height <= Width ? (double)Width / Height : (double)Height / Width;

			glMatrixMode(GL_PROJECTION);
			glLoadIdentity();

			//====== ��������� ������ ������������� ��������
			gluPerspective(m_AngleView, dAspect, 0.1f, 200);

			//====== ��������� �������������� ���������
			glViewport(0, 0, Width, Height);
		}

		protected override void PrepareScene()
		{
			SetProjection();
			//====== ������ ����� �������� ������� OpenGL
			glEnable(GL_LIGHTING);			// ����� ���������
			//====== ����� ������ ���� �������� �����
			glEnable(GL_LIGHT0);
			//====== ���������� ��������� ������� (��� Z)
			glEnable(GL_DEPTH_TEST);
			//====== ���������� ��������� ���� ��������� �����������
			glEnable(GL_COLOR_MATERIAL);

			//====== ������������� ���� ����
			SetBkColor();

			//====== ������� ����������� � ���������� � ������
			DrawScene();
		}

		private void SetLight()
		{
			//====== ��� ����������� ����������� ���������
			//====== ��� ���������� ����� ��������
			//====== ��� ����� ���������� ���������
			glLightModeli(GL_LIGHT_MODEL_TWO_SIDE, 1);

			//====== ������� ��������� ���������
			//====== ������� �� �������� �������
			float[] fPos =
			{
				(m_LightParam[0]-50) * 10 * m_fRangeX/100,
				(m_LightParam[1]-50) * 10 * m_fRangeY/100,
				(m_LightParam[2]-50) * 5 * m_fRangeZ/100,
				1.0f
			};
			glLightfv(GL_LIGHT0, GL_POSITION, fPos);

			//====== ������������� ����������� ���������
			float f = m_LightParam[3] / 100.0f;
			float[] fAmbient = { f, f, f, 0.0f };
			glLightfv(GL_LIGHT0, GL_AMBIENT, fAmbient);

			//====== ������������� ����������� �����
			f = m_LightParam[4] / 100.0f;
			float[] fDiffuse = { f, f, f, 0.0f };
			glLightfv(GL_LIGHT0, GL_DIFFUSE, fDiffuse);

			//====== ������������� ����������� �����
			f = m_LightParam[5] / 100.0f;
			float[] fSpecular = { f, f, f, 0.0f };
			glLightfv(GL_LIGHT0, GL_SPECULAR, fSpecular);

			//====== ���������� �������� ���������
			//====== ��� ������ ��������� �����
			f = m_LightParam[6] / 100.0f;
			float[] fAmbMat = { f, f, f, 0.0f };
			glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, fAmbMat);

			f = m_LightParam[7] / 100.0f;
			float[] fDifMat = { f, f, f, 1.0f };
			glMaterialfv(GL_FRONT_AND_BACK, GL_DIFFUSE, fDifMat);

			f = m_LightParam[8] / 100.0f;
			float[] fSpecMat = { f, f, f, 0.0f };
			glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, fSpecMat);

			//====== ���������� ���������
			float fShine = 128 * m_LightParam[9] / 100.0f;
			glMaterialf(GL_FRONT_AND_BACK, GL_SHININESS, fShine);

			//====== ��������� ����� ����������
			f = m_LightParam[10] / 100.0f;
			float[] fEmission = { f, f, f, 0.0f };
			glMaterialfv(GL_FRONT_AND_BACK, GL_EMISSION, fEmission);
		}

		private void SetBkColor()
		{
			//====== ����������� ����� �� ��� ����������
			float red = m_BkClr.R / 255.0f,
					green = m_BkClr.G / 255.0f,
					blue = m_BkClr.B / 255.0f;
			//====== ��������� ����� ���� (��������) ����
			glClearColor(red, green, blue, 0.0f);

			//====== ���������������� ��������
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
		}

		/// <summary>
		/// �������� ������ �������� ������
		/// </summary>
		public void DrawScene()
		{
			//====== �������� ������ �������� ������
			glNewList(1, GL_COMPILE);

			//====== ��������� ������ ����������
			//====== ���������� ����� ���������
			glPolygonMode(GL_FRONT_AND_BACK, m_FillMode);

			//====== ������� ������������� �������
			uint nx = m_xSize - 1,
					nz = m_zSize - 1;

			//====== ����� ������� �������� ���������
			if (m_bQuad)
				glBegin(GL_QUADS);

			//====== ���� ������� �� ����� ����������� (��� Z)
			for (uint z = 0, i = 0; z < nz; z++, i++)
			{
				//====== ��������� �������� ����������
				//====== �� ������ ������ �����
				if (!m_bQuad)
					glBegin(GL_QUAD_STRIP);

				//====== ���� ������� ����� ��� X
				for (uint x = 0; x < nx; x++, i++)
				{
					// i, j, k, n - 4 ������� ������ ��������� ���
					// ������ � ����������� ������ ������� �������

					int j = (int)i + (int)m_xSize,	// ������ ���� � ������� Z
						k = j + 1,					// ������ ���� �� ���������
						n = (int)i + 1; 				// ������ ���� ������

					//=== ����� ��������� 4-� ������ �� ����������
					float
						xi = m_cPoints[i].x,
						yi = m_cPoints[i].y,
						zi = m_cPoints[i].z,

						xj = m_cPoints[j].x,
						yj = m_cPoints[j].y,
						zj = m_cPoints[j].z,

						xk = m_cPoints[k].x,
						yk = m_cPoints[k].y,
						zk = m_cPoints[k].z,

						xn = m_cPoints[n].x,
						yn = m_cPoints[n].y,
						zn = m_cPoints[n].z,

					//=== ���������� �������� ������� ������
						ax = xi - xn,
						ay = yi - yn,

						by = yj - yi,
						bz = zj - zi,

					//====== ���������� ������� �������
						vx = ay * bz,
						vy = -bz * ax,
						vz = ax * by,

					//====== ������ �������
						v = (float)Math.Sqrt(vx * vx + vy * vy + vz * vz);

					//====== ���������� ������� �������
					vx /= v;
					vy /= v;
					vz /= v;

					//====== ������� ������� �������
					glNormal3f(vx, vy, vz);

					// ����� �������� ����������� �����������������
					if (m_bQuad)
					{
						//====== ����� ������ ��������������
						//====== � ����������� ������ ������� �������
						glColor3f(0.20f, 0.80f, 1.0f);
						glVertex3f(xi, yi, zi);
						glColor3f(0.60f, 0.70f, 1.0f);
						glVertex3f(xj, yj, zj);
						glColor3f(0.70f, 0.90f, 1.0f);
						glVertex3f(xk, yk, zk);
						glColor3f(0.70f, 0.80f, 1.0f);
						glVertex3f(xn, yn, zn);
					}
					else
					//====== ����� �������� ������� �����������������
					{
						glColor3f(0.90f, 0.90f, 1.0f);
						glVertex3f(xi, yi, zi);
						glColor3f(0.50f, 0.80f, 1.0f);
						glVertex3f(xj, yj, zj);
						if (x == nx - 1)
						{
							glColor3f(0.90f, 0.90f, 1.0f);
							glVertex3f(xn, yn, zn);
							glColor3f(0.50f, 0.80f, 1.0f);
							glVertex3f(xk, yk, zk);
						}
					}
				}
				//====== ��������� ���� ������ GL_QUAD_STRIP
				if (!m_bQuad)
					glEnd();
			}
			//====== ��������� ���� ������ GL_QUADS
			if (m_bQuad)
				glEnd();

			//====== ��������� ������ ������ OpenGL
			glEndList();
		}

		private void DefaultGraphic()
		{
			uint size = 33;
			//====== ��������� ����� ��� �������� ������
			float[] buff = new float[size];

			//====== ����� ����� �� ������� ������ ����� �����
			uint nz = size - 1,
					nx = size - 1;

			//=== �������������� ��������� ������������ ���������
			double fi = Math.Atan(1.0) * 6,
					kx = fi / nx,
					kz = fi / nz;

			//====== � ������� ����� ������� �� ����� �����
			//=== ��������� � �������� � ����� ������ ���� float
			int k = 0;
			uint i, j;
			for (i = 0; i < m_zSize; i++)
			{
				for (j = 0; j < m_xSize; j++)
				{
					buff[k] = (float)(Math.Sin(kz * (i - nz / 2.0)) * Math.Sin(kx * (j - nx / 2.0)));
					k++;
				}
			}
			//InitGraphic(buff);
		}

		private void InitGraphic(float[][] buff)
		{
			//====== ������� ����� �����
			m_xSize = (uint)buff.Length;
			m_zSize = (uint)buff[0].Length;

			// ������ ������ ��� �������� �������� �������
			ulong nSize = m_xSize * m_zSize;

			//====== �������� ������������� ������� m_cPoints
			//====== ��������� � ����������� ������ ������
			//====== �������� �� ������������������
			if (m_xSize < 2 || m_zSize < 2 || m_xSize * m_zSize != nSize)
			{
				MessageBox.Show("������ �������������");
				return;
			}

			//====== �������� ������ ����������
			//====== ��� ���� ��� ������ �����������
			m_cPoints = new CPoint3D[m_xSize * m_zSize];

			if (m_cPoints.Length == 0)
			{
				MessageBox.Show("�� �������� ���������� ������");
				return;
			}

			//====== ���������� � ����� ������� �� ������
			//====== � �������� ���������������
			float x, z,
				//====== ��������� ������ ��������
			fMinY = buff[0][0],
			fMaxY = buff[0][0],
			right = (m_xSize - 1) / 2.0f,
			left = -right,
			rear = (m_zSize - 1) / 2.0f,
			front = -rear,
			range = (right + rear) / 2.0f;

			uint n;

			//====== ���������� ������� ������������� �������
			m_fRangeY = range;
			m_fRangeX = (float)m_xSize;
			m_fRangeZ = (float)m_zSize;

			//====== �������� ������ ����� ��� Z
			m_zTrans = -1.5f * m_fRangeZ;

			//====== ���������� ���������� ����� (X-Z)
			//====== � ��������� � ���������� Y �� ������
			int i = 0,
				j = 0;
			for (z = front, i = 0, n = 0; i < m_zSize; i++, z += 1.0f)
			{
				for (x = left, j = 0; j < m_xSize; j++, x += 1.0f, n++)
				{
					if (buff[i][j] > fMaxY)
						fMaxY = buff[i][j];			// ���������� �� ��������
					else if (buff[i][j] < fMinY)
						fMinY = buff[i][j];			// ���������� �� �������
					m_cPoints[n] = new CPoint3D(x, z, buff[i][j]);
				}
			}

			//====== ��������������� �������
			float zoom = fMaxY > fMinY ? range / (fMaxY - fMinY) : 1.0f;

			for (n = 0; n < m_xSize * m_zSize; n++)
			{
				m_cPoints[n].y = zoom * (m_cPoints[n].y - fMinY) - range / 2.0f;
			}

			//====== ����������� ��������� �����
		}
		protected override void OnMouseDown(MouseEventArgs e)
		{
			m_dx = m_dy = 0;

			SetCapture(this.Handle);
			m_bCaptured = true;
			m_pt.X = e.X; m_pt.Y = e.Y;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (m_bCaptured)
			{
				m_dy = (float)(e.Y - m_pt.Y) / 40.0f;
				m_dx = (float)(e.X - m_pt.X) / 40.0f;

				if (e.Button == MouseButtons.Right)
					m_zTrans += m_dx + m_dy;
				else
				{
					m_AngleX += m_dy;
					m_AngleY += m_dx;
				}
				m_pt.X = e.X; m_pt.Y = e.Y;
				Invalidate();
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			m_yTrans -= e.Delta / 50.0f;
			Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (m_bCaptured)
			{
				if (Math.Abs(m_dx) > 0.5f || Math.Abs(m_dy) > 0.5f)
					timer.Start();
				else
					timer.Stop();

				m_bCaptured = false;
				ReleaseCapture();
			}
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			m_AngleX += m_dy;// ����������� ���� ��������
			if (m_AngleX > 360)
				m_AngleX -= 360;
			m_AngleY += m_dx;
			if (m_AngleY > 360)
				m_AngleY -= 360;

			Invalidate();
		}
	}
}

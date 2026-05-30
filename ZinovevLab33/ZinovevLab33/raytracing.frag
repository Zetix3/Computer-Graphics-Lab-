#version 430 core
#define EPSILON 0.001
#define BIG 1000000.0

uniform vec2 uResolution;
uniform int uDepth;

out vec4 FragColor; 
in vec3 glPosition; 

const int DIFFUSE = 1; 
const int REFLECTION = 2;
const int REFRACTION = 3;

const int DIFFUSE_REFLECTION = 1; 
const int MIRROR_REFLECTION = 2;
const int GLASS_REFRACTION = 3;

const int STACK_SIZE = 32;

struct SCamera 
{ 
	vec3 Position;
	vec3 View;
	vec3 Up;
	vec3 Side; 
	vec2 Scale; 	
}; 

struct SRay 
{ 
	vec3 Origin; 
	vec3 Direction; 
};

struct SSphere 
{ 
	vec3 Center;
	float Radius;
	int MaterialIdx; 
};

struct STriangle 
{ 
	vec3 v1; 
	vec3 v2; 
	vec3 v3; 
	int MaterialIdx; 
};

struct SIntersection 
{ 
	float Time;
	vec3 Point;
	vec3 Normal;
	vec3 Color;
	vec4 LightCoeffs; 
	float ReflectionCoef; 
	float RefractionCoef;
	float IOR;
	int MaterialType;
};

struct SLight 
{ 
	vec3 Position;
};

struct SMaterial 
{
	vec3 Color; 
	vec4 LightCoeffs; 
	float ReflectionCoef; 
	float RefractionCoef; 
	float IOR;
	int MaterialType; 
};

struct STracingRay 
{ 
	SRay ray;
	float contribution;
	int depth; 
};

struct SRayStack
{
    STracingRay stack[STACK_SIZE];
    int top;  
};

SRayStack TSRayStack;

void pushRay(STracingRay ray)
{
    if (TSRayStack.top < STACK_SIZE - 1)
    {
        TSRayStack.top++;
        TSRayStack.stack[TSRayStack.top] = ray;
    }
}

STracingRay popRay()
{
    STracingRay result = TSRayStack.stack[TSRayStack.top];
    TSRayStack.top--;
    return result;
}

bool isEmpty()
{
    return TSRayStack.top == -1;
}

void initStack()
{
    TSRayStack.top = -1;
}

SLight light; 
SMaterial materials[6];

STriangle triangles[26]; 
SSphere spheres[2];


SRay GenerateRay ( SCamera uCamera ) 
{ 
	float aspect = uResolution.x / uResolution.y;
	vec2 coords = glPosition.xy * uCamera.Scale;
	coords.x *= aspect;
	vec3 direction = uCamera.View + uCamera.Side * coords.x + uCamera.Up * coords.y;
	return SRay ( uCamera.Position, normalize(direction) ); 
} 

SCamera initializeDefaultCamera() 
{ 
	SCamera camera;

	camera.Position = vec3(0.0, 0.0, -8.0); 
	camera.View = vec3(0.0, 0.0, 1.0); 
	camera.Up = vec3(0.0, 1.0, 0.0); 
	camera.Side = vec3(1.0, 0.0, 0.0);
	camera.Scale = vec2(1.0); 


	return camera;
}

void initializeDefaultScene(out STriangle triangles[26], out SSphere spheres[2]) 
{ 
	/* left wall */
	triangles[0].v1 = vec3(-5.0,-5.0,-5.0); 
	triangles[0].v2 = vec3(-5.0, 5.0, 5.0); 
	triangles[0].v3 = vec3(-5.0, 5.0,-5.0);
	triangles[0].MaterialIdx = 0; 

	triangles[1].v1 = vec3(-5.0,-5.0,-5.0); 
	triangles[1].v2 = vec3(-5.0,-5.0, 5.0); 
	triangles[1].v3 = vec3(-5.0, 5.0, 5.0);
	triangles[1].MaterialIdx = 0; 
	
	/* back wall */
	triangles[2].v1 = vec3(-5.0,-5.0, 5.0);
	triangles[2].v2 = vec3( 5.0,-5.0, 5.0);
	triangles[2].v3 = vec3(-5.0, 5.0, 5.0); 
	triangles[2].MaterialIdx = 0; 

	triangles[3].v1 = vec3( 5.0, 5.0, 5.0); 
	triangles[3].v2 = vec3(-5.0, 5.0, 5.0); 
	triangles[3].v3 = vec3( 5.0,-5.0, 5.0); 
	triangles[3].MaterialIdx = 0;

	/* right wall */
	triangles[4].v1 = vec3( 5.0, -5.0, 5.0); 
	triangles[4].v2 = vec3( 5.0, 5.0, -5.0); 
	triangles[4].v3 = vec3( 5.0, 5.0, 5.0); 
	triangles[4].MaterialIdx = 0;

	triangles[5].v1 = vec3( 5.0,-5.0, 5.0);
	triangles[5].v2 = vec3( 5.0,-5.0, -5.0);
	triangles[5].v3 = vec3( 5.0, 5.0, -5.0); 
	triangles[5].MaterialIdx = 0; 

	/* down */
	triangles[6].v1 = vec3(-5.0,-5.0, -5.0);
	triangles[6].v2 = vec3( 5.0,-5.0, 5.0);
	triangles[6].v3 = vec3( -5.0, -5.0, 5.0); 
	triangles[6].MaterialIdx = 2; 

	triangles[7].v1 = vec3( -5.0, -5.0, -5.0); 
	triangles[7].v2 = vec3( 5.0, -5.0, -5.0); 
	triangles[7].v3 = vec3( 5.0,-5.0, 5.0); 
	triangles[7].MaterialIdx = 2;

	/* upper */
	triangles[8].v1 = vec3(-5.0, 5.0, 5.0);
	triangles[8].v2 = vec3( 5.0, 5.0, -5.0);
	triangles[8].v3 = vec3( -5.0, 5.0, -5.0); 
	triangles[8].MaterialIdx = 2; 

	triangles[9].v1 = vec3( -5.0, 5.0, 5.0); 
	triangles[9].v2 = vec3( 5.0, 5.0, 5.0); 
	triangles[9].v3 = vec3( 5.0, 5.0, -5.0); 
	triangles[9].MaterialIdx = 2;
	
	/** SPHERES **/
	spheres[0].Center = vec3(-1.0,-1.0,-2.0); 
	spheres[0].Radius = 1.7; 
	spheres[0].MaterialIdx = 1; 
	
	spheres[1].Center = vec3(2.0,1.0,2.0); 
	spheres[1].Radius = 1.0; 
	spheres[1].MaterialIdx = 3;

	/* CUBE */
	/* left */
	triangles[10].v1 = vec3( 1.0, -5.0, -3.0); 
	triangles[10].v2 = vec3( 1.0, -3.0, -1.0); 
	triangles[10].v3 = vec3( 1.0, -3.0, -3.0);
	triangles[10].MaterialIdx = 4; 

	triangles[11].v1 = vec3( 1.0, -5.0, -3.0); 
	triangles[11].v2 = vec3( 1.0, -5.0, -1.0); 
	triangles[11].v3 = vec3( 1.0, -3.0, -1.0);
	triangles[11].MaterialIdx = 4; 

	/* back */
	triangles[12].v1 = vec3( 1.0, -5.0, -1.0);
	triangles[12].v2 = vec3( 3.0, -5.0, -1.0);
	triangles[12].v3 = vec3( 1.0, -3.0, -1.0); 
	triangles[12].MaterialIdx = 4; 

	triangles[13].v1 = vec3( 3.0, -3.0, -1.0); 
	triangles[13].v2 = vec3( 1.0, -3.0, -1.0); 
	triangles[13].v3 = vec3( 3.0, -5.0, -1.0); 
	triangles[13].MaterialIdx = 4;

	/* right */
	triangles[14].v1 = vec3( 3.0, -5.0, -1.0); 
	triangles[14].v2 = vec3( 3.0, -3.0, -3.0); 
	triangles[14].v3 = vec3( 3.0, -3.0, -1.0); 
	triangles[14].MaterialIdx = 4;

	triangles[15].v1 = vec3( 3.0, -5.0, -1.0);
	triangles[15].v2 = vec3( 3.0, -5.0, -3.0);
	triangles[15].v3 = vec3( 3.0, -3.0, -3.0); 
	triangles[15].MaterialIdx = 4; 

	/* down  */
	triangles[16].v1 = vec3( 1.0, -5.0, -3.0);
	triangles[16].v2 = vec3( 3.0, -5.0, -1.0);
	triangles[16].v3 = vec3( 1.0, -5.0, -1.0); 
	triangles[16].MaterialIdx = 4; 

	triangles[17].v1 = vec3( 1.0, -5.0, -3.0); 
	triangles[17].v2 = vec3( 3.0, -5.0, -3.0); 
	triangles[17].v3 = vec3( 3.0, -5.0, -1.0); 
	triangles[17].MaterialIdx = 4;

	/* upper */
	triangles[18].v1 = vec3( 1.0, -3.0, -1.0);
	triangles[18].v2 = vec3( 3.0, -3.0, -3.0);
	triangles[18].v3 = vec3( 1.0, -3.0, -3.0); 
	triangles[18].MaterialIdx = 4; 

	triangles[19].v1 = vec3( 1.0, -3.0, -1.0); 
	triangles[19].v2 = vec3( 3.0, -3.0, -1.0); 
	triangles[19].v3 = vec3( 3.0, -3.0, -3.0);  
	triangles[19].MaterialIdx = 4;

	/* front */
	triangles[20].v1 = vec3( 1.0, -5.0, -3.0);
	triangles[20].v2 = vec3( 3.0, -5.0, -3.0);
	triangles[20].v3 = vec3( 3.0, -3.0, -3.0); 
	triangles[20].MaterialIdx = 4; 

	triangles[21].v1 = vec3( 1.0, -5.0, -3.0); 
	triangles[21].v2 = vec3( 3.0, -3.0, -3.0); 
	triangles[21].v3 = vec3( 1.0, -3.0, -3.0); 
	triangles[21].MaterialIdx = 4;

	/* TETRAHEDRON */
	triangles[22].v1 = vec3(-3.9, -4.0, -2.5);
	triangles[22].v2 = vec3(-2.9, -4.0, -4.0);
	triangles[22].v3 = vec3(-2.9, -2.5, -3.0);
	triangles[22].MaterialIdx = 5;

	triangles[23].v1 = vec3(-2.9, -4.0, -4.0);
	triangles[23].v2 = vec3(-1.9, -4.0, -2.5);
	triangles[23].v3 = vec3(-2.9, -2.5, -3.0);
	triangles[23].MaterialIdx = 5;

	triangles[24].v1 = vec3(-1.9, -4.0, -2.5);
	triangles[24].v2 = vec3(-3.9, -4.0, -2.5);
	triangles[24].v3 = vec3(-2.9, -2.5, -3.0);
	triangles[24].MaterialIdx = 5;

	triangles[25].v1 = vec3(-3.9, -4.0, -2.5);
	triangles[25].v2 = vec3(-2.9, -4.0, -4.0);
	triangles[25].v3 = vec3(-1.9, -4.0, -2.5);
	triangles[25].MaterialIdx = 5;
}

void initializeDefaultLightMaterials(out SLight light, out SMaterial materials[6]) 
{ 
	light.Position = vec3(0.0, 2.0, -4.0f); 
	vec4 lightCoefs = vec4(0.4,0.9,0.0,512.0); 
	materials[0].Color = vec3(0.0, 1.0, 0.0); 
	materials[0].LightCoeffs = vec4(lightCoefs); 
	materials[0].ReflectionCoef = 0.5; 
	materials[0].RefractionCoef = 1.0; 
	materials[0].IOR = 1.5;
	materials[0].MaterialType = DIFFUSE_REFLECTION; 
	
	materials[1].Color = vec3(0.0, 0.0, 1.0);
	materials[1].LightCoeffs = vec4(lightCoefs); 
	materials[1].ReflectionCoef = 0.3;
	materials[1].RefractionCoef = 0.4;
	materials[1].IOR = 1.3;
	materials[1].MaterialType = GLASS_REFRACTION;
	
	materials[2].Color = vec3(1.0, 0.0, 0.0);
    materials[2].LightCoeffs = vec4(0.5, 0.5, 0.0, 32.0);
    materials[2].ReflectionCoef = 0.5;
	materials[2].RefractionCoef = 1.7;
	materials[2].IOR = 1.3;
    materials[2].MaterialType = DIFFUSE_REFLECTION;

	materials[3].Color = vec3(1.0, 1.0, 0.0);
    materials[3].LightCoeffs = vec4(0.5, 0.5, 0.0, 32.0);
    materials[3].ReflectionCoef = 0.5;
	materials[3].RefractionCoef = 1.7;
	materials[3].IOR = 1.3;
    materials[3].MaterialType = MIRROR_REFLECTION;

	materials[4].Color = vec3(1.0, 1.0, 0.0);
    materials[4].LightCoeffs = vec4(0.5, 0.5, 0.0, 32.0);
    materials[4].ReflectionCoef = 0.5;
    materials[4].RefractionCoef = 1.0;
	materials[4].IOR = 1.3;
    materials[4].MaterialType = DIFFUSE_REFLECTION;

	for(int i = 5; i < 6; i++)
    {
        materials[i].Color = vec3(0.0, 1.0, 1.0);
        materials[i].LightCoeffs = vec4(0.5, 0.5, 0.0, 32.0);
        materials[i].ReflectionCoef = 0.5;
        materials[i].RefractionCoef = 1.0;
		materials[i].IOR = 1.3;
        materials[i].MaterialType = DIFFUSE_REFLECTION;
    }
}

bool IntersectSphere ( SSphere sphere, SRay ray, float start, float final, out float time ) 
{ 
	ray.Origin -= sphere.Center;
	float A = dot ( ray.Direction, ray.Direction ); 
	float B = dot ( ray.Direction, ray.Origin ); 
	float C = dot ( ray.Origin, ray.Origin ) - sphere.Radius * sphere.Radius; 
	float D = B * B - A * C; 
	if ( D > 0.0 ) 
	{ 
		D = sqrt ( D ); 
		//time = min ( max ( 0.0, ( -B - D ) / A ), ( -B + D ) / A );
		float t1 = ( -B - D ) / A;
		float t2 = ( -B + D ) / A; 
		if(t1 < 0 && t2 < 0) return false; 
		if(min(t1, t2) < 0) { time = max(t1,t2); return true; } 
		time = min(t1, t2); 
		return true; 
	} 
	return false; 
}

bool IntersectTriangle (SRay ray, vec3 v1, vec3 v2, vec3 v3, out float time ) 
{

	time = -1; 
	vec3 A = v2 - v1;
	vec3 B = v3 - v1; 
	vec3 N = cross(A, B); 

	float NdotRayDirection = dot(N, ray.Direction); 
	if (abs(NdotRayDirection) < 0.001) return false; 

	float d = dot(N, v1); 
	float t = -(dot(N, ray.Origin) - d) / NdotRayDirection; 

	if (t < 0) return false; 

	vec3 P = ray.Origin + t * ray.Direction; 

	vec3 C; 

	vec3 edge1 = v2 - v1; 
	vec3 VP1 = P - v1; 
	C = cross(edge1, VP1);
	if (dot(N, C) < 0) return false; 
 
	vec3 edge2 = v3 - v2;
	vec3 VP2 = P - v2; 
	C = cross(edge2, VP2); 
	if (dot(N, C) < 0) return false;

	vec3 edge3 = v1 - v3; 
	vec3 VP3 = P - v3; 
	C = cross(edge3, VP3); 
	if (dot(N, C) < 0) return false; 

	time = t; 
	return true; 
}

bool Raytrace ( SRay ray, SSphere spheres[2], STriangle triangles[26], 
SMaterial materials[6], float start, float final, inout SIntersection intersect ) 
{ 
	bool result = false; 
	float test = start;
	intersect.Time = final;
	int matIdx1;
	int matIdx2;

	for(int i = 0; i < 2; i++) 
	{ 
		SSphere sphere = spheres[i]; 
		
		if( IntersectSphere (sphere, ray, start, final, test ) && test < intersect.Time ) 
		{ 
			matIdx1 = spheres[i].MaterialIdx;
			intersect.Time = test; 
			intersect.Point = ray.Origin + ray.Direction * test; 
			intersect.Normal = normalize ( intersect.Point - spheres[i].Center ); 
			intersect.Color = materials[matIdx1].Color; 
			intersect.LightCoeffs = materials[matIdx1].LightCoeffs; 
			intersect.ReflectionCoef = materials[matIdx1].ReflectionCoef; 
			intersect.RefractionCoef = materials[matIdx1].RefractionCoef; 
			intersect.IOR = materials[matIdx1].IOR;
			intersect.MaterialType = materials[matIdx1].MaterialType; 
			result = true; 
		} 
	}

	for(int i = 0; i < 26; i++) 
	{ 
		STriangle triangle = triangles[i]; 
		if(IntersectTriangle(ray, triangle.v1, triangle.v2, triangle.v3, test) && test < intersect.Time) 
		{ 
			matIdx2 = triangles[i].MaterialIdx;
			intersect.Time = test; 
			intersect.Point = ray.Origin + ray.Direction * test; 
			intersect.Normal = normalize(cross(triangle.v1 - triangle.v2, triangle.v3 - triangle.v2)); 
			intersect.Color = materials[matIdx2].Color; 
			intersect.LightCoeffs = materials[matIdx2].LightCoeffs;  
			intersect.ReflectionCoef = materials[matIdx2].ReflectionCoef;
			intersect.RefractionCoef = materials[matIdx2].RefractionCoef;
			intersect.IOR = materials[matIdx2].IOR;
			intersect.MaterialType = materials[matIdx2].MaterialType; 
			result = true; 
		} 
	} 
	return result; 
}

vec3 Phong ( SIntersection intersect, SLight currLight, float shadow, SCamera uCamera) 
{ 
	vec3 light = normalize ( currLight.Position - intersect.Point ); 
	float diffuse = max(dot(light, intersect.Normal), 0.0);
	vec3 view = normalize(uCamera.Position - intersect.Point);
	vec3 reflected = reflect( -view, intersect.Normal );
	float specular = pow(max(dot(reflected, light), 0.0), intersect.LightCoeffs.w); 
	return intersect.LightCoeffs.x * intersect.Color +
		intersect.LightCoeffs.y * diffuse * intersect.Color * shadow + 
		intersect.LightCoeffs.z * specular * vec3(1.0) * shadow; 
}

float Shadow(SLight currLight, SIntersection intersect) 
{ 
	float shadowing = 1.0; 
	vec3 direction = normalize(currLight.Position - intersect.Point); 
	float distanceLight = distance(currLight.Position, intersect.Point); 
	SRay shadowRay = SRay(intersect.Point + direction * EPSILON, direction); 
	SIntersection shadowIntersect;
	shadowIntersect.Time = BIG;   
	if(Raytrace(shadowRay, spheres, triangles, materials, 0.001, distanceLight, shadowIntersect)) 
	{
		shadowing = 0.0; 
	} 
	return shadowing; 
}

void main ( void ) 
{  
	initStack();
	initializeDefaultScene ( triangles, spheres );
	initializeDefaultLightMaterials(light, materials);
	SCamera uCamera = initializeDefaultCamera();

	vec3 resultColor = vec3(0,0,0);

	SRay ray = GenerateRay( uCamera);

	STracingRay trRay = STracingRay(ray, 1, 0);
	pushRay(trRay);
	
	while(!isEmpty())
    {
        STracingRay currentRay = popRay();
        ray = currentRay.ray;
        
        SIntersection intersect; 
        intersect.Time = BIG;
        
        float start = 0.001;  
        float final = BIG;
        
        if (Raytrace(ray, spheres, triangles, materials, start, final, intersect)) 
        {
            switch(intersect.MaterialType)
            {
                case DIFFUSE_REFLECTION:
                {
                    float shadowing = Shadow(light, intersect);
                    resultColor += currentRay.contribution * Phong(intersect, light, shadowing, uCamera);
                    break;
                }
                case MIRROR_REFLECTION:
                {
                    if(intersect.ReflectionCoef < 1.0)
                    {
                        float contribution = currentRay.contribution * (1.0 - intersect.ReflectionCoef);
                        float shadowing = Shadow(light, intersect);
                        resultColor += contribution * Phong(intersect, light, shadowing, uCamera);
                    }
                    
                    vec3 reflectDirection = reflect(ray.Direction, intersect.Normal);
                    float contribution = currentRay.contribution * intersect.ReflectionCoef;
                    
                    STracingRay reflectRay;
                    reflectRay.ray = SRay(intersect.Point + reflectDirection * EPSILON, reflectDirection);
                    reflectRay.contribution = contribution;
                    reflectRay.depth = currentRay.depth + 1;
                    
                    
                    if (currentRay.depth < uDepth)  
                    {
                        pushRay(reflectRay);
                    }
                    break;
                }
                case GLASS_REFRACTION:
				{
					float totalNonDiffuse = intersect.ReflectionCoef + intersect.RefractionCoef;
					float contribution;
					if (totalNonDiffuse < 1.0)
					{
						contribution = currentRay.contribution * (1.0 - totalNonDiffuse);
						float shadowing = Shadow(light, intersect);
						resultColor += contribution * Phong(intersect, light, shadowing, uCamera);
					}
    
					float eta;
					vec3 normal = intersect.Normal;
    
					if (dot(ray.Direction, intersect.Normal) < 0.0)
					{
						eta = 1.0 / intersect.IOR;
					}
					else
					{
						eta = intersect.IOR;
						normal = -normal;  
					}
    
					vec3 refractDirection = refract(ray.Direction, normal, eta);
    
					if (refractDirection != vec3(0.0))
					{
						contribution = currentRay.contribution * intersect.RefractionCoef;
        
						STracingRay refractRay;
						refractRay.ray = SRay(intersect.Point + refractDirection * EPSILON, refractDirection);
						refractRay.contribution = contribution;
						refractRay.depth = currentRay.depth + 1;
        
						if (currentRay.depth < uDepth)
						{
							pushRay(refractRay);
						}

					}
					else
					{
						contribution = currentRay.contribution * (intersect.ReflectionCoef + intersect.RefractionCoef);
					}
					
					vec3 reflectDirection = reflect(ray.Direction, intersect.Normal);
					
					STracingRay reflectRay;
					reflectRay.ray = SRay(intersect.Point + reflectDirection * EPSILON, reflectDirection);
					reflectRay.contribution = contribution;
					reflectRay.depth = currentRay.depth + 1;
        
					if (currentRay.depth < uDepth)
					{
						pushRay(reflectRay);
					}
					
					break;
				}
            }
        }
        
    }
	
	FragColor = vec4 (resultColor, 1.0);
}
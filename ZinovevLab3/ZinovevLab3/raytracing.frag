#version 430 core
out vec4 FragColor; 
in vec3 glPosition; 
void main ( void ) 
{  
	//FragColor = vec4 ( abs(glPosition.xy), 0, 1.0); 
	FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}
//
// Created by edgar on 9/26/2023.
//
#include <glew.h>
#include <glfw3.h>
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>
#include <vector>

#include "Mesh.h"
#include "ObjectReader.h"
#include "Shader.h"
#include "Window.h"

Window mainWindow;
std::vector<Shader> shaderList;
std::vector<GLfloat> vertex;
std::vector<unsigned int> faces;
MeshColor *characterMesh;

// Vertex Shader
static const char *vShader = "shaders/shaderpersonaje.vert";
static const char *fShader = "shaders/shaderpersonaje.frag";

void CreateShaders()
{
	auto shader1 = new Shader(); // shader para usar ndices: objetos: cubo y  pirmide
	shader1->CreateFromFiles(vShader, fShader);
	shaderList.push_back(*shader1);
}

void CreateCharacter()
{
	characterMesh = new MeshColor();
	characterMesh->CreateMeshColor(vertex, faces, vertex.size(), faces.size());
}

int main()
{
	mainWindow = Window(800, 800);
	mainWindow.Initialise();
	CreateShaders();

	ObjectReader reader("ObjParsed/vertex.txt", "ObjParsed/faces.txt");
	reader.LoadAll();

	vertex = reader.getVertex();
	faces = reader.getFaces();
	CreateCharacter();

	GLuint uniformProjection;
	GLuint uniformModel;
	glm::mat4 projection = glm::ortho(-1.0f, 1.0f, -1.0f, 1.0f, 0.1f, 100.0f);
	
	shaderList[0].useShader();
	uniformModel = shaderList[0].getModelLocation();
	uniformProjection = shaderList[0].getProjectLocation();
	glUniformMatrix4fv((GLint) uniformProjection, 1, GL_FALSE, glm::value_ptr(projection));
	glm::mat4 model(1.0);
	model = glm::translate(model, {0.0f, 0.0f, -4.0f});
	model = glm::scale(model, {0.149f, 0.147f, 0.147f});
	model = glm::rotate(model, glm::radians(90.0f), {1.0f, 0.0f, 0.0f});
	glUniformMatrix4fv((GLint) uniformModel, 1, GL_FALSE, glm::value_ptr(model));
	
	// Loop mientras no se cierra la ventana
	while (!mainWindow.getShouldClose())
	{
		glfwPollEvents();
		glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
		shaderList[0].useShader();
		
		characterMesh->RenderMeshColor();

		glUseProgram(0);
		mainWindow.swapBuffers();
	}
	return 0;
}
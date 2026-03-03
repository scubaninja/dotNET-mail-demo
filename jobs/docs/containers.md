# Containers

You can run this application locally or on a VM with Go and Mage, or build it as a container.

We build our `jobs` container using the Dockerfiles, [Dockerfile](../Dockerfile) and [dev.Dockerfile](../dev.Dockerfile).

The [build-and-publish.yaml](../.github/workflows/build-and-publish.yaml) GitHub Action builds and publishes the `jobs` container, from the `latest` branch, to GitHub Container Registry.

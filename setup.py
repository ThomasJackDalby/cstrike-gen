from setuptools import setup

setup(name='cstrike_gen',
      version='0.1',
      description='CLI application for procedural generation of Counter-Strike Source maps',
      url='http://github.com/thomasjackdalby/cstrike_gen',
      author='Tom Dalby',
      author_email='thomasjackdalby@gmail.com',
      license='MIT',
      packages=['cstrike_gen'],
      zip_safe=False,
      entry_points = {
        'console_scripts': ['cstrike_gen=cstrike_gen.__main__:main'],
    })
import wave
import struct
import math
import random

def generate_shotgun(filename):
    sample_rate = 44100
    duration = 0.5
    num_samples = int(sample_rate * duration)
    
    with wave.open(filename, 'w') as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(sample_rate)
        
        for i in range(num_samples):
            t = i / sample_rate
            # Ruído branco para a explosão
            noise = random.uniform(-1, 1)
            # Envelope de queda (deixa o som sumir)
            envelope = math.exp(-6 * t)
            
            value = int(noise * envelope * 32767 * 0.5)
            f.writeframesraw(struct.pack('<h', value))

def generate_footstep(filename):
    sample_rate = 44100
    duration = 0.1
    num_samples = int(sample_rate * duration)
    
    with wave.open(filename, 'w') as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(sample_rate)
        
        for i in range(num_samples):
            t = i / sample_rate
            # Senoide de baixa frequência para o "impacto"
            freq = 60
            v = math.sin(2 * math.pi * freq * t)
            # Envelope muito rápido
            envelope = math.exp(-20 * t)
            
            value = int(v * envelope * 32767 * 0.3)
            f.writeframesraw(struct.pack('<h', value))

def generate_monster_death(filename):
    sample_rate = 44100
    duration = 0.7
    num_samples = int(sample_rate * duration)
    
    with wave.open(filename, 'w') as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(sample_rate)
        
        for i in range(num_samples):
            t = i / sample_rate
            # Frequência que cai (rosnado)
            freq = 150 * (1 - t)
            v = math.sin(2 * math.pi * freq * t) * random.uniform(0.5, 1.0) # Adiciona distorção
            envelope = math.exp(-3 * t)
            
            value = int(v * envelope * 32767 * 0.4)
            f.writeframesraw(struct.pack('<h', value))

print("Gerando sons...")
generate_shotgun("Content/shotgun.wav")
generate_footstep("Content/footstep.wav")
generate_monster_death("Content/death.wav")
print("Sons gerados com sucesso!")

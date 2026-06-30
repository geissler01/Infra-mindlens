import os
import sys

# Asegurar path para imports relativos
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from app import lambda_handler

def main():
    print("==========================================================")
    print("    SIMULADOR PRE-SESIÓN - FLASH BRIEFING                 ")
    print("==========================================================")
    
    # Fechas objetivo para simular la cita
    # Supongamos que la cita del paciente fue el lunes 29 de junio de 2026.
    target_dates = ["2026-06-29"]
    
    for fecha_str in target_dates:
        print("\n" + "="*50)
        print(f"⏩ VIAJANDO EN EL TIEMPO: CITA EL -> {fecha_str}")
        print("="*50)
        
        event = {
            "target_date": fecha_str
        }
        
        # Ejecutar la lambda de forma sincrona
        lambda_handler(event, None)
        
    print("\n==========================================================")
    print(" SIMULACION DE FLASH BRIEFING COMPLETADA CON EXITO")
    print(" La tabla 'pre_session_reports' está ahora poblada.")
    print("==========================================================")

if __name__ == "__main__":
    main()

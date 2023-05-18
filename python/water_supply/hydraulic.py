import math

supply_temperature = 70  # Температура подачи в градусах Цельсия
return_temperature = 40  # Температура обратки в градусах Цельсия
design_flow_rate = 1.5  # Проектная расходная мощность в л/с

# Расчет гидравлического сопротивления трубы
def calculate_pipe_pressure_drop(flow_rate, pipe_diameter, pipe_length):
    pressure_drop = 0.26 * (flow_rate ** 2) * pipe_length / (pipe_diameter ** 5)
    return pressure_drop

# Расчет гидравлического сопротивления компонентов системы
def calculate_component_pressure_drop(flow_rate, component_loss_coefficient):
    pressure_drop = component_loss_coefficient * (flow_rate ** 2)
    return pressure_drop

# Расчет гидравлического сопротивления всей системы
def calculate_total_pressure_drop(pipe_pressure_drop, component_pressure_drop):
    total_pressure_drop = pipe_pressure_drop + sum(component_pressure_drop)
    return total_pressure_drop

# Расчет диаметра трубы на основе проектной расходной мощности
def calculate_pipe_diameter(flow_rate):
    pipe_diameter = math.sqrt(flow_rate / (math.pi * 3.6))
    return pipe_diameter

# Расчет гидравлического расчета системы отопления
def perform_hydraulic_calculation():
    pipe_diameter = calculate_pipe_diameter(design_flow_rate)

    pipe_pressure_drop = calculate_pipe_pressure_drop(design_flow_rate, pipe_diameter, 100)

    component_loss_coefficients = [0.1, 0.05, 0.2]  
    component_pressure_drop = [calculate_component_pressure_drop(design_flow_rate, coeff) for coeff in component_loss_coefficients]

    total_pressure_drop = calculate_total_pressure_drop(pipe_pressure_drop, component_pressure_drop)

    print("Диаметр трубы: {} мм".format(round(pipe_diameter, 2)))
    print("Гидравлическое сопротивление трубы: {} Па".format(round(pipe_pressure_drop, 2)))
    print("Гидравлическое сопротивление компонентов: {} Па".format([round(pressure_drop, 2) for pressure_drop in component_pressure_drop]))
    print("Общее гидравлическое сопротивление системы: {} Па".format(round(total_pressure_drop, 2)))

perform_hydraulic_calculation()
